import pytest
import sys

from CrimeData.locationHandling.address import parse_address



def test_address_parsing_with_simple_address():

    expected_number = 4001
    expected_street = "S PONDEROSA ST"

    address = f"{expected_number} {expected_street}"
    number, street = parse_address(address)

    assert number is expected_number
    assert street is street


def test_address_parsing_with_apartment():

    # with aprartment number 
    expected_number = 4001
    expected_street = "S PONDEROSA ST"

    address = f"{expected_number}-181 {expected_street}"
    number, street = parse_address(address)

    assert number is expected_number
    assert street is street

    # with apartment letter
    expected_number = 366
    expected_street = "E CABO LN"

    address = f"{expected_number}-A {expected_street}"
    number, street = parse_address(address)

    assert number is expected_number
    assert street is street

    # with apartment letter and number
    expected_number = 602
    expected_street = "N RUBY ST"

    address = f"{expected_number}-4A {expected_street}"
    number, street = parse_address(address)

    assert number is expected_number
    assert street is street