import json
import time
import os
import requests
from models import Base, Address
from engine import CONNECTION_STRING, create_database_engine, insert_data
from stored_procedures import create_stored_procedures
from sqlalchemy.orm import Session

backstone_host = os.getenv("BACKSTONE_HOST", "backstone")

def insert_addresses(engine, file):

    json_file_path = os.path.join(os.path.dirname(__file__), file)

    addresses = []
    hashes_set = set()
    with open(json_file_path) as f:
        for row in f.readlines():
            address_json = json.loads(row)
            hash = address_json["properties"]["hash"]

            if hash not in hashes_set:
                address = create_address_from_json(address_json)
                if address:
                    addresses.append(address)
                hashes_set.add(hash)

    unique_key = ["number", "street", "unit", "city", "zipcode"]

    insert_data(engine, addresses, Address, natural_key=unique_key)

def create_address_from_json(address_json):

    address_data = address_json["properties"]
    coordinates = address_json["geometry"]["coordinates"]

    if not address_data["number"] or not address_data["street"] or not address_data["city"] or not address_data["postcode"]:
        return None

    address = {
        "number": int(address_data["number"]),
        "street": address_data["street"].lower(),
        "unit": address_data["unit"].lower() if address_data["unit"] else "",
        "city": address_data["city"].lower(),
        "state": "MO".lower(),
        "zipcode": address_data["postcode"],
        "longitude": coordinates[0] ,
        "latitude": coordinates[1],
        "geohash": None
    }

    return address

def hash_addresses():
    request = f"http://{backstone_host}/Address/HashUnhashed"
    print(request)
    result = requests.get(request)
    if result.status_code == 200:
        print("Successfully hashed addresses")
    else:
        print("Failed to hash addresses")

if __name__ == "__main__":
    print("Connection string: ", CONNECTION_STRING)

    engine = create_database_engine(CONNECTION_STRING)

    print("Creating database tables...")
    Base.metadata.create_all(engine)
    print("Tables created!")
    time.sleep(3)

    create_stored_procedures(engine)
    print("Created stored procedures")

    insert_addresses(engine, "boone-county-addresses.geojson")
    print("Inserted addresses")

    time.sleep(10)
    hash_addresses()
