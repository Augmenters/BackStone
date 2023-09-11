from datetime import datetime

from Python.CrimeData.celery_app import celery_app
from Python.CrimeData.scrapers.scraper import collect_BSCO_crime_data   
from Python.CrimeData.locationHandling.address import getCrimeCordinates

@celery_app.task
def BSCO_collect():

    from Python.CrimeData.celery_app import engine
    
    start_date = datetime(2023, 1, 1)
    end_date = datetime(2023, 1, 15)

    collect_BSCO_crime_data(start_date, end_date, engine)

@celery_app.task
def getCordinates(): 
    from Python.CrimeData.celery_app import engine
    getCrimeCordinates(engine)