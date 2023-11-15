import sqlalchemy as s
import pygeohash as pgh
from collections import defaultdict


def getCrimes(engine):
    query = s.text("""
        SELECT crimes.id, crimes.time_slot_id,crime_coordinates.longitude, crime_coordinates.latitude 
        FROM crimes
        INNER JOIN crime_coordinates ON crime_coordinates.crime_id = crimes.id""")
    result = engine.execute(query).fetchall()
    crimes = [dict(row) for row in result]
    return crimes


def getHash(engine, crime_id):
    qt = "SELECT crime_coordinates.grid_hash \
        FROM crime_coordinates \
        WHERE crime_coordinates.crime_id=" + str(crime_id) + ";"
    query = s.text(qt)
    result = engine.execute(query).fetchall()
    data = [dict(row) for row in result]
    if len(data) == 1:
        return data
    else:
        return None

def computeStats(engine):
    print("Getting all the crimes to compute new stats")
    crimes = getCrimes(engine)
    map = defaultdict(int)

    print("Computing stats")
    for entry in crimes:
        time_slot_id = int(entry["time_slot_id"])
        geo_hash = getHash(engine, int(entry["id"]))
        if geo_hash is not None:
            map[(time_slot_id, str(geo_hash[0]["grid_hash"]))] += 1

    print(f"Map: {map}")

    for key, count in map.items():
        time_slot_id, geohash = key

        qt = f"INSERT INTO time_slot_grids(time_slot_id, grid_hash, crime_count) \
            VALUES ({str(time_slot_id)}, '{geohash}', {str(count)});"
        
        engine.execute(s.text(qt))
