from celery import Celery

BROKER_URL = 'redis://localhost:6380/0'
BACKEND_URL = 'redis://localhost:6380/1'
tasks = ['tasks']

celery_app = Celery('tasks', broker=BROKER_URL, backend=BACKEND_URL, include=tasks)


