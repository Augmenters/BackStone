import sqlalchemy as s
import time
from sqlalchemy import create_engine
from sqlalchemy.exc import OperationalError
import re
from Database.engine import insert_data
from Database.models import t_crime_coordinates

CONNECTION_STRING = f"postgresql+psycopg2://backstone_user:password@127.0.0.1:5432/base_db"


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


def get_data():
    engine = create_database_engine(CONNECTION_STRING)

    query = s.text("""
        SELECT crime_addresses.crime_id, crime_addresses.address
        FROM crime_addresses
        LEFT JOIN crime_coordinates ON crime_addresses.crime_id = crime_coordinates.crime_id
        WHERE crime_coordinates.crime_id IS NULL;
    """)

    result = engine.execute(query).fetchall()

    data = [dict(row) for row in result]
    return data


def getCordinates(engine, address):
    pattern = r'\d+\s+\w+\s+\w+'

    if re.match(pattern, address):
        # Gets street numebr
        number = address.split()[0]

        # formats address stirng
        address.strip()
        street = re.sub(r'^\d+\s*', '', address)

        qt = "SELECT addresses.longitude, addresses.latitude FROM addresses WHERE addresses.number='" + number + "' AND addresses.street LIKE '%" + street.lower() + "%';"
        query = s.text(qt)
        result = engine.execute(query).fetchall()

        if len(result) == 1:
            return result
        else:
            return None

def processData(engine, data):
    crimeCordinates = []

    for entry in data:
        address = entry["address"]
        cordinates = getCordinates(engine, address)
        print(cordinates)

        if cordinates:
            cordinateEntry = {
                "crime_id": entry["crime_id"],
                "latitude": cordinates[0][0],
                "longitude": cordinates[0][1],
            }

            crimeCordinates.append(cordinateEntry)
    return crimeCordinates

def getCrimeCordinates(engine):
    data = get_data()
    crimeCordinates = processData(engine, data)
    print(crimeCordinates)