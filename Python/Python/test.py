import pytest
import sys
import random
import string


from CrimeData.locationHandling.address import parse_BSCO_address, parse_CPD_address

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
