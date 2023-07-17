
sleep 1m

python -c "from Python.CrimeData.tasks import BSCO_collect; BSCO_collect.delay()"

celery -A Python.CrimeData.celery_app:celery_app worker -l info