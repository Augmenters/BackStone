import sqlalchemy as s
import re


def get_data(engine):
    query = s.text("""
        SELECT crime_addresses.crime_id, crime_addresses.address
        FROM crime_addresses
        LEFT JOIN crime_coordinates ON crime_addresses.crime_id = crime_coordinates.crime_id
        WHERE crime_coordinates.crime_id IS NULL;
    """)

    result = engine.execute(query).fetchall()

    data = [dict(row) for row in result]
    return data

def parse_address(address):

    number = None
    street = None


    return number, street


def getCordinates(engine, address):
    
    pattern = r'\d+\s+\w+\s+\w+'

    if re.match(pattern, address):
        # Gets street numebr
        number = address.split()[0]

        # formats address stirng
        address.strip()
        street = re.sub(r'^\d+\s*', '', address).strip().lower()
        if street.startswith("block"):
            street = street[5:].strip()

        qt = "SELECT addresses.longitude, addresses.latitude, addresses.geohash FROM addresses WHERE addresses.number='" + number + "' AND addresses.street LIKE '%" + street.lower() + "%';"
        query = s.text(qt)
        result = engine.execute(query).fetchall()

        if len(result) == 1:
            print(f"Address matched and found: {address}. Address: {address}. Number: {number}. Street: {street}.")
            return result
        else:
            print(f"Address not found in database. Address: {address}. Number: {number}. Street: {street}.")
            return None
        
    print(f"Address did not match regex: {address}")


def insertCrimeData(engine, entry):
    print(entry)
    qt = "INSERT INTO crime_coordinates(crime_id, longitude, latitude, grid_hash) \
            VALUES (" + entry["crime_id"] + "," + entry["longitude"] + ","\
            + entry["latitude"] + ",'" + entry["geohash"] + "');"
    query = s.text(qt)
    engine.execute(query)


def processData(engine, data):
    for entry in data:
        address = entry["address"]
        cordinates = getCordinates(engine, address)

        if cordinates:
            cordinateEntry = {
                "crime_id": str(entry["crime_id"]),
                "latitude": str(cordinates[0][0]),
                "longitude": str(cordinates[0][1]),
                "geohash": str(cordinates[0][2]),
            }
            insertCrimeData(engine, cordinateEntry)

def getCrimeCordinates(engine):
    print("Getting crime addresses that need resolved")
    data = get_data(engine)
    
    print("Resolving crime addresses to coordinates and inserting them")
    crimeCordinates = processData(engine, data)
