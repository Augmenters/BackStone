from sqlalchemy import BigInteger, Column, ForeignKey, Integer, SmallInteger, String, Table
from sqlalchemy.orm import relationship
from sqlalchemy.ext.declarative import declarative_base

Base = declarative_base()
metadata = Base.metadata


t_addresses = Table(
    'addresses', metadata,
    Column('id', BigInteger),
    Column('number', Integer),
    Column('street', String(255)),
    Column('unit', String(255)),
    Column('city', String(255)),
    Column('state', String(255)),
    Column('zipcode', SmallInteger),
    Column('longitude', String(255)),
    Column('latitude', String(255)),
    Column('geohash', String(255))
)


class Agency(Base):
    __tablename__ = 'agencies'

    id = Column(SmallInteger, primary_key=True)
    name = Column(String(255), nullable=False)


class TimeSlot(Base):
    __tablename__ = 'time_slots'

    id = Column(SmallInteger, primary_key=True)
    day_of_week = Column(String(255))
    time_of_day = Column(String(255))


class Crime(Base):
    __tablename__ = 'crimes'

    id = Column(BigInteger, primary_key=True)
    agency_id = Column(ForeignKey('agencies.id'))
    incident_id = Column(BigInteger)
    time_slot_id = Column(ForeignKey('time_slots.id'))
    # TODO: Add actual crime columns
    crime_columns = Column(String(255))

    agency = relationship('Agency')
    time_slot = relationship('TimeSlot')


class CrimeAddress(Crime):
    __tablename__ = 'crime_addresses'

    crime_id = Column(ForeignKey('crimes.id'), primary_key=True)
    # TODO: Add actual address columns
    address = Column(String(255))


class TimeSlotGrid(Base):
    __tablename__ = 'time_slot_grids'

    time_slot_id = Column(ForeignKey('time_slots.id'), primary_key=True, nullable=False)
    grid_hash = Column(String(255), primary_key=True, nullable=False)
    # TODO: Add actual status column
    stats = Column(String(255))

    time_slot = relationship('TimeSlot')


t_crime_coordinates = Table(
    'crime_coordinates', metadata,
    Column('crime_id', ForeignKey('crimes.id')),
    Column('longitude', String(255)),
    Column('latitude', String(255))
)