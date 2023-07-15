from Python.CrimeData.celery_app import celery_app

@celery_app.task
def add(x, y):
    print('Adding %d + %d' % (x, y))
    print("Result: %d" % (x + y))
    return x + y


