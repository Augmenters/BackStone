import os
import time

from sqlalchemy import create_engine
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
            engine = create_engine(CONNECTION_STRING, **kwargs)
            connection = engine.connect()
            connection.close()
            return engine
        except OperationalError as e:
            print("Postgres is not ready. Waiting...")
            time.sleep(2)
            continue
