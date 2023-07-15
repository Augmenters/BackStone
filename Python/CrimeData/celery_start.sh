# dispatch a task
python -c "from tasks import collect_bsco; collect_bsco.delay()"

# start a worker
celery -A celery_app:celery_app worker -l info