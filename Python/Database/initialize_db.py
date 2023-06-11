from models import Base
from engine import CONNECTION_STRING, create_database_engine

if __name__ == "__main__":
    print("Connection string: ", CONNECTION_STRING)

    engine = create_database_engine(CONNECTION_STRING)

    print("Creating database tables...")
    Base.metadata.create_all(engine)
    print("Tables created!")