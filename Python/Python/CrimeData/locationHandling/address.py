import sqlalchemy as s
import re


def get_data(engine, agency_id):
    query = s.text(f"""
        SELECT ca.crime_id, ca.address
        FROM crime_addresses ca
        LEFT JOIN crime_coordinates cc ON ca.crime_id = cc.crime_id
        INNER JOIN crimes c ON ca.crime_id = c.id 
        WHERE cc.crime_id IS NULL
        AND c.agency_id = {agency_id}; 
    """)

    result = engine.execute(query).fetchall()

    data = [dict(row) for row in result]
    return data

def parse_BSCO_address(address):

    number = None
    street = None

    pattern = r'(?P<number>\d+)(?:-[A-Za-z0-9]+)?\s+(?P<street>.+)$'

    match = re.match(pattern, address)
    if match:
        # Extract the data using group names
        number = match.group('number')
        street = match.group('street')

    return number, street

def parse_CPD_address(address):

    number = None
    street = None

    pattern = r'\d+\s+\w+\s+\w+'

    if re.match(pattern, address):
        # Gets street numebr
        number = address.split()[0]

        # formats address stirng
        address.strip()
        street = re.sub(r'^\d+\s*', '', address).strip().lower()
        if street.startswith("block"):
            street = street[5:].strip()

    return number, street


def find_address_in_db(engine, number, street):

    result = []
    qt = "SELECT addresses.longitude, addresses.latitude, addresses.geohash FROM addresses WHERE addresses.number='" + number + "' AND addresses.street LIKE '%" + street.lower() + "%' LIMIT 1;"
    query = s.text(qt)
    result = engine.execute(query).fetchall()

    return result


def find_nearby_address(engine, number, street):

    qt = f"SELECT addresses.longitude, addresses.latitude, addresses.geohash, addresses.number FROM addresses WHERE addresses.street LIKE '%{street.lower()}%' ORDER BY ABS(addresses.number - {number}) LIMIT 1;"
    query = s.text(qt)
    result = engine.execute(query).fetchall()

    if len(result) == 1:
        return result
    else:
        return []
  

def getBscoCordinates(engine, address):
    
    number, street = parse_BSCO_address(address)
    if number and street:

        result = find_address_in_db(engine, number, street)

        if len(result) == 1:
            #print(f"BCSO Address matched and found: {address}. Address: {address}. Number: {number}. Street: {street}.")
            return result
        else:
            result = find_nearby_address(engine, number, street)
            if len(result) == 1:
                return result
            
            print(f"BCSO Address not found in database. Address: {address}. Number: {number}. Street: {street.lower()}.")
            return None
        
    #print(f"BCSO Address did not match regex: {address}")

def getCpdCoordinates(engine, address):
    
    street, number = parse_CPD_address(address)

    if street and number:

        result = find_address_in_db(engine, number, street)
        
        if len(result) == 1:
            #print(f"CPD Address matched and found: {address}. Address: {address}. Number: {number}. Street: {street}.")
            return result
        else:
            result = find_nearby_address(engine, number, street)
            if len(result) == 1:
                return result
            
            print(f"CPD Address not found in database. Address: {address}. Number: {number}. Street: {street}.")
            return None
        
    #print(f"CPD Address did not match regex: {address}")


def insertCrimeData(engine, entry):
    qt = "INSERT INTO crime_coordinates(crime_id, longitude, latitude, grid_hash) \
            VALUES (" + entry["crime_id"] + "," + entry["longitude"] + ","\
            + entry["latitude"] + ",'" + entry["geohash"] + "');"
    query = s.text(qt)
    engine.execute(query)


def processData(engine, data, agency_id):
    for entry in data:
        address = entry["address"]
        
        coordinates = None
        if agency_id == 1:
            coordinates = getBscoCordinates(engine, address)
        elif agency_id == 2:
            coordinates = getCpdCoordinates(engine, address)
        elif agency_id == 3:
            coordinates = getMupdCoordinates(engine, address)
        else:
            raise Exception (f"Agency id is invalid: {agency_id}")

        if coordinates:
            cordinateEntry = {
                "crime_id": str(entry["crime_id"]),
                "latitude": str(coordinates[0][0]),
                "longitude": str(coordinates[0][1]),
                "geohash": str(coordinates[0][2]),
            }
            insertCrimeData(engine, cordinateEntry)

def getCrimeCordinates(engine):

    bsco_agency_id = 1
    print("Getting BCSO crime addresses that need resolved")
    data = get_data(engine, bsco_agency_id)
    
    print("Resolving BCSO crime addresses to coordinates and inserting them")
    processData(engine, data, bsco_agency_id)

    cpd_agency_id = 2
    print("Getting CPD crime addresses that need resolved")
    data = get_data(engine, cpd_agency_id)
    print(f"CPD addresses that need resolved: {len(data)}")
    
    print("Resolving CPD crime addresses to coordinates and inserting them")
    processData(engine, data, cpd_agency_id)

