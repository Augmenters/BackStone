
sleep 1m

python -c "from Python.CrimeData.tasks import BSCO_collect; from Python.CrimeData.locationHandling.address import getCrimeCordinates ; from celery import chain; chain(BSCO_collect.si(),getCordinates.si()).apply_async();"

celery -A Python.CrimeData.celery_app:celery_app worker -l info