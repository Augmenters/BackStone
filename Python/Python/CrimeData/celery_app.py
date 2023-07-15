import os
from sqlalchemy.pool import  StaticPool


from celery import Celery
from celery.signals import worker_process_init, worker_process_shutdown

host = os.environ.get('REDIS_HOST', 'localhost')
port = os.environ.get('REDIS_PORT', 6379)

BROKER_URL = f'redis://{host}:{port}/0'
BACKEND_URL = f'redis://{host}:{port}/1'
tasks = ['Python.CrimeData.tasks']

celery_app = Celery('tasks', broker=BROKER_URL, backend=BACKEND_URL, include=tasks)



engine = None
@worker_process_init.connect
def init_worker(**kwargs):

    from Python.Database.engine import CONNECTION_STRING, create_database_engine 

    print("Create database connection for worker")
    print(f"Connection string: {CONNECTION_STRING}")

    global engine
    engine = create_database_engine(CONNECTION_STRING, poolclass=StaticPool)

@worker_process_shutdown.connect
def shutdown_worker(**kwargs):
    global engine
    if engine:
        print('Closing database connectionn for worker')
        engine.dispose()
