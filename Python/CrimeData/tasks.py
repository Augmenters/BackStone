from celery_app import celery_app
from scrapers.scraper import run_BSCO_scrape

@celery_app.task
def collect_bsco():
    print("Collecting boone country sheriffs office")

    print("No data is collected yet because the scraper isn't setup to insert into the db")









