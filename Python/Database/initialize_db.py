import json
import os
from models import Base, Address
from engine import CONNECTION_STRING, create_database_engine
from sqlalchemy.orm import Session

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
                addresses.append(address)
                hashes_set.add(hash)

    with Session(engine) as session:
        print(f"Inserting {len(addresses)} addresses")
        session.add_all(addresses)
        session.commit()


def create_address_from_json(address_json):

    address_data = address_json["properties"]
    coordinates = address_json["geometry"]["coordinates"]

    address = Address(
        id = address_data["hash"],
        number = int(address_data["number"]) if address_data["number"] else None,
        street = address_data["street"],
        unit = address_data["unit"] if address_data["unit"] else None,
        city = address_data["city"],
        state = None,
        zipcode = int(address_data["postcode"]) if address_data["postcode"] else None,
        longitude = coordinates[0] ,
        latitude = coordinates[1],
        geohash = None
    )

    return address



if __name__ == "__main__":
    print("Connection string: ", CONNECTION_STRING)

    engine = create_database_engine(CONNECTION_STRING)

    print("Creating database tables...")
    Base.metadata.create_all(engine)
    print("Tables created!")

    insert_addresses(engine, "boone-county-addresses.geojson")
    print("Inserted addresses")