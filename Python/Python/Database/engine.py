import os
import time

from sqlalchemy import create_engine
from sqlalchemy.dialects import postgresql
from sqlalchemy.exc import OperationalError

# FIX ME: We aren't able to get these values from the environment
user = os.getenv("POSTGRES_USER")
password = os.getenv("POSTGRES_PASSWORD")
db_name = os.getenv("POSTGRES_DB")
host = os.getenv("POSTGRES_HOST", "database")

CONNECTION_STRING = f"postgresql+psycopg2://{user}:{password}@{host}:5432/{db_name}"


def create_database_engine(url, **kwargs):  

    while True:
        try:
            print("Connecting to Postgres...")
            engine = create_engine(url, **kwargs)
            connection = engine.connect()
            connection.close()
            return engine
        except OperationalError as e:
            print("Postgres is not ready. Waiting...")
            time.sleep(2)
            continue

def remove_duplicates_by_uniques(data, uniques):

    unique_values = {}

    unique_data = []

    #Deal with null data being passed. 
    if not uniques:
        return data

    for x in data:

        # creates a key out of the uniques
        key = "_".join([str(x[unique]) for unique in uniques])

        # if a KeyError does not occur then a dict with those values has already been processed
        # if a KeyError occurs a dict with those values has not been found yet
        try:
            unique_values[key]
            continue
        except KeyError:
            unique_values[key] = 1
            unique_data.append(x)

    return unique_data


def insert_data(engine, data, table, natural_key, return_columns=[], on_conflict_update = True):

    if isinstance(data, list) is False:
        
        # if a dict is passed to data then 
        # convert it to a list with one value
        if isinstance(data, dict) is True:
            data = [data]
        
        else:
            print("Data must be a list or a dict")
            return False

    if len(data) == 0:
        
        return False

    if isinstance(data[0], dict) is False: 
        print("Must be list of dicts")
        return False
    
    data = remove_duplicates_by_uniques(data, natural_key)

    returning_args = [getattr(table, column) for column in return_columns]

    # creates insert on table
    # that returns cols specificed in returning_args
    # and inserts the data specified in data
    # NOTE: if return_columns does not have an values this still works
    stmnt = postgresql.insert(table).returning(*returning_args).values(data)

    if on_conflict_update:

        # create a dict that the on_conflict_do_update method requires to be able to map updates whenever there is a conflict. See sqlalchemy docs for more explanation and examples: https://docs.sqlalchemy.org/en/14/dialects/postgresql.html#updating-using-the-excluded-insert-values
        setDict = {}
        for key in data[0].keys():
                setDict[key] = getattr(stmnt.excluded, key)
            
        stmnt = stmnt.on_conflict_do_update(
            #This might need to change
            index_elements=natural_key,
            
            #Columns to be updated
            set_ = setDict
        )

    else:
        stmnt = stmnt.on_conflict_do_nothing(
            index_elements=natural_key
        )

    if return_columns:
        with engine.connect() as connection:
            result = connection.execute(stmnt).fetchall()

            data = [dict(row) for row in result]
            
            return data
    else:

        with engine.connect() as connection:
            connection.execute(stmnt)
            
        return True

