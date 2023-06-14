import os

from celery import Celery

host = os.environ.get('REDIS_HOST', 'localhost')
port = os.environ.get('REDIS_PORT', 6379)

BROKER_URL = f'redis://{host}:{port}/0'
BACKEND_URL = f'redis://{host}:{port}/1'
tasks = ['tasks']

celery_app = Celery('tasks', broker=BROKER_URL, backend=BACKEND_URL, include=tasks)


