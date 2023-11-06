
sleep 1m

python -c "
from celery import chain, group
from Python.CrimeData.tasks import BSCO_collect, getCordinates, CPD_Collect, MUPD_Collect, compute_stats

parallel_scraping_tasks = group(BSCO_collect.si(), CPD_Collect.si(), MUPD_Collect.si())

pipeline = chain(
    parallel_scraping_tasks,
    getCordinates.si(),
    compute_stats.si()
)

pipeline.apply_async()
"

celery -A Python.CrimeData.celery_app:celery_app worker -l info