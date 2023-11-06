import pytest
import sys
import random
import string
from datetime import datetime


from Python.CrimeData.locationHandling.address import parse_BSCO_address, parse_CPD_address
from Python.CrimeData.scrapers.scraper import convert_CPD_time_to_datetime, datetime_to_timeslot_id, split_into_monthly_collection_intervals



def get_random_string(length):
    # choose from all lowercase letter
    letters = string.ascii_lowercase
    result_str = ''.join(random.choice(letters) for i in range(length))


def test_parse_BSCO_address_with_conventional_address():
    
    expected_number = str(random.randint(1, 4000))
    expected_street = f"S {get_random_string(8)} ST"

    address = f"{expected_number} {expected_street}"
    number, street = parse_BSCO_address(address)

    assert number == expected_number
    assert street == street


def test_parse_BSCO_address_with_apartment_number():
    
    expected_number = str(random.randint(1, 4000))
    expected_street = f"S {get_random_string(8)} ST"
    apartment_number = random.randint(1, 99)

    address = f"{expected_number}-{apartment_number} {expected_street}"
    number, street = parse_BSCO_address(address)

    assert number == expected_number
    assert street == street
    

def test_parse_BSCO_address_with_apartment_letter():
    
    expected_number = str(random.randint(1, 4000))
    expected_street = f"S {get_random_string(8)} ST"
    apartment_letter = get_random_string(1)

    address = f"{expected_number}-{apartment_letter} {expected_street}"
    number, street = parse_BSCO_address(address)

    assert number == expected_number
    assert street == street


def test_parse_CPD_address_with_conventional_address():
    
    expected_number = str(random.randint(1, 4000))
    expected_street = f"S {get_random_string(8)} ST"

    address = f"{expected_number} {expected_street}"
    number, street = parse_CPD_address(address)

    assert number == expected_number
    assert street == street

def test_parse_CPD_address_with_block():
    
    expected_number = str(random.randint(1, 4000))
    expected_street = f"S {get_random_string(8)} ST"

    address = f"{expected_number} BLOCK {expected_street}"
    number, street = parse_CPD_address(address)

    assert number == expected_number
    assert street == street

def test_convert_CPD_time_to_datetime():

    CPD_time = "1/14/2023 12:40:32 PM"
    expected_datetime = datetime(year=2023, month=1, day=14, hour=12, minute=40, second=32)

    assert convert_CPD_time_to_datetime(CPD_time) == expected_datetime

@pytest.mark.parametrize("datetime, expected_day_of_week", [
    (datetime(year=2023, month=10, day=29, hour=1), "Sunday"),
    (datetime(year=2023, month=10, day=30, hour=1), "Monday"),
    (datetime(year=2023, month=10, day=31, hour=1), "Tuesday"),
    (datetime(year=2023, month=11, day=1, hour=1), "Wednesday"),
    (datetime(year=2023, month=11, day=2, hour=1), "Thursday"),
    (datetime(year=2023, month=11, day=3, hour=1), "Friday"),
    (datetime(year=2023, month=11, day=4, hour=1), "Saturday"),
])
def test_datetime_to_timeslot_id_time_day_of_week_conversion(datetime, expected_day_of_week):
    day_of_week, _ = datetime_to_timeslot_id(datetime)
    assert day_of_week == expected_day_of_week


@pytest.mark.parametrize("datetime, expected_time_of_day", [
    (datetime(year=2023, month=11, day=1, hour=0), 1),
    (datetime(year=2023, month=11, day=1, hour=6), 2),
    (datetime(year=2023, month=11, day=1, hour=12), 3),
    (datetime(year=2023, month=11, day=1, hour=18), 4),
    (datetime(year=2023, month=11, day=1, hour=1), 1),
    (datetime(year=2023, month=11, day=1, hour=7), 2),
    (datetime(year=2023, month=11, day=1, hour=13), 3),
    (datetime(year=2023, month=11, day=1, hour=19), 4),
])
def test_datetime_to_timeslot_id_time_day_of_week_conversion(datetime, expected_time_of_day):
    _, time_of_day = datetime_to_timeslot_id(datetime)
    assert time_of_day == expected_time_of_day


def test_split_into_monthly_collection_intervals_with_less_than_month_interval():

    start_date = datetime(year=2023, month=11, day=1)
    end_date = datetime(year=2023, month=11, day=12)
    expected_intervals = [{"start": start_date, "end": end_date}]

    assert split_into_monthly_collection_intervals(start_date, end_date) == expected_intervals


def test_split_into_monthly_collection_intervals_with_dates_in_different_months():

    start_date = datetime(year=2023, month=10, day=20)
    end_date = datetime(year=2023, month=11, day=12)
    expected_intervals = [{"start": start_date, "end": end_date}]

    assert split_into_monthly_collection_intervals(start_date, end_date) == expected_intervals


def test_split_into_monthly_collection_intervals_with_dates_that_span_multiple_months():

    start_date = datetime(year=2023, month=8, day=20)
    end_date = datetime(year=2023, month=11, day=12)
    expected_intervals = [
        {"start": start_date, "end": datetime(year=2023, month=9, day=19)},
        {"start": datetime(year=2023, month=9, day=19), "end": datetime(year=2023, month=10, day=19)},
        {"start": datetime(year=2023, month=10, day=19), "end": end_date}
    ]

    assert split_into_monthly_collection_intervals(start_date, end_date) == expected_intervals

