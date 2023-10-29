from datetime import datetime

from Python.CrimeData.celery_app import celery_app
from Python.CrimeData.scrapers.scraper import collect_BSCO_crime_data, run_CPD_scrape  
from Python.CrimeData.locationHandling.address import getCrimeCordinates
from Python.CrimeData.stats.stats import computeStats

start_date = datetime(2023, 1, 1)
end_date = datetime.now()

@celery_app.task
def BSCO_collect():
    print("Collecting BSCO...")

    from Python.CrimeData.celery_app import engine
    collect_BSCO_crime_data(start_date, end_date, engine)

@celery_app.task
def getCordinates(): 
    print("Resolving addresses to coordinates...")
    from Python.CrimeData.celery_app import engine
    getCrimeCordinates(engine)


@celery_app.task
def CPD_Collect():
    from Python.CrimeData.celery_app import engine
    print("Collecting CPD...")
    run_CPD_scrape(start_date, end_date, engine)

@celery_app.task
def MUPD_Collect():
    from Python.CrimeData.celery_app import engine
    print("Collecitng MUPD...")

@celery_app.task
def compute_stats():
    from Python.CrimeData.celery_app import engine
    print("Computing stats...")
    computeStats(engine)

