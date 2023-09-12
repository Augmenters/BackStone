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
    data = get_data(engine)
    crimeCordinates = processData(engine, data)
    print(crimeCordinates)
