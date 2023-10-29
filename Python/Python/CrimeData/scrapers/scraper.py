import requests
import calendar
import datetime
import re
from Python.CrimeData.scrapers import crimedict
import numpy
import csv
from bs4 import BeautifulSoup
from dateutil.relativedelta import relativedelta
import datetime
import sqlalchemy as s
from Python.Database.models import Crime, CrimeAddress
from Python.Database.engine import insert_data

AGENCY_ID = 1

# def grab_indice_from_wrapper(wrapper, index_desired):
#     return [wrapper[0][index_desired], wrapper[1][index_desired], wrapper[2][index_desired], wrapper[3][index_desired]]

def create_crime_dict(incident_id, agency_id, time_slot_id, type):

    return {
        "incident_id": incident_id,
        "agency_id": agency_id,
        "time_slot_id": time_slot_id,
        "type": type,
    }


def in_BCSO_dict(query):
    for key in crimedict.bcsodict.keys():
        if query in key:
            return True
    return False


def get_BCSO_dict_value(query):
    correct_key = None
    for key in crimedict.bcsodict.keys():
        if query in key:
            correct_key = key
            break
    if correct_key is not None:
        return crimedict.bcsodict.get(correct_key)
    else:
        return "Invalid"


# def convert_time(bad_time):
#     if bad_time[-2:] == "AM" and bad_time[:2] == "12":
#         return "00" + bad_time[2:-2]
#     elif bad_time[-2:] == "AM":
#         return "0" + bad_time[:-2]
#     elif bad_time[-2:] == "PM" and bad_time[:2] != "12":
#         if bad_time[:2] == "11":
#             return "23" + bad_time[1:-2]
#         else:
#             twentyfour_time = str(int(bad_time[:1]) + 12)
#             return twentyfour_time + bad_time[1:-2]
#     else:
#         return bad_time[:-2]


# Send a GET request to the webpage
def run_BSCO_scrape(startDate, endDate, rowCount, engine):
    #Date format MM/DD/YYYY
    #Row count 1 < x < 10000
    #Will crash if more than 10000, the sheriffs website cannot handle the load
    if(not startDate or not endDate or rowCount == ""):
        return None
    
    startDateStr = startDate.strftime('%m/%d/%Y')
    endDateStr = endDate.strftime('%m/%d/%Y')

    url = 'https://report.boonecountymo.org/mrcjava/servlet/SH01_MP.I00070s'

    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36",
    }

    incidentType = ""
    # "" = view all
    address = ""
    location = ''

    parameters = {
        'startDate' : startDateStr,
        'endDate' : endDateStr,
        'rls_EXTERNAL': 'CT',
        'val_EXTERNAL' : incidentType,
        'val_ADDR01' : address,
        'rls_CALCULA006': 'EQ',
        'val_CALCULA006' : location,
        'max_rows' : rowCount,
        'rls_CALLDATE': 'RG',
        'val_CALLDATE': '' + startDateStr + ' ' + endDateStr,
        'rls_CALLTIME': 'EQ',
        'val_CALLTIME': '',
        'rls_INNUM': 'EQ',
        'val_INNUM': '',
        'rls_REPORT': 'EQ',
        'val_REPORT': '',
        'rls_ADDR01': 'CT',
        'val_ADDR01': '',
        'rls_APTLOT': 'EQ',
        'val_APTLOT': '',
        'slnk': '0',
        'sort_col': 'CALLDATE',
        'sort_typ': '0',
        'cur_sort_col': '',
        'reorder': 'N',
        'pageName': '',
        'preview': '0',
        'debug': '0',
        'pageNum': '1',
        'total_pages': '13935',
        'total_rows': '418033',
        'ajax_form': '1',
        'auto_refresh': '0',
        'excel': '0',
        'use_udview': '0',
        'val_CALCULA006': '',
    }

    response = requests.post(url, headers=headers, data=parameters)
    soup = BeautifulSoup(response.text, "html.parser")
    incident_instance = soup.findAll("td", attrs={"data-th": "Incident"})
    addresses = soup.findAll("td", attrs={"data-th": "Address"})
    call_dates = soup.findAll("td", attrs={"data-th": "Call Date"})
    call_times = soup.findAll("td", attrs={"data-th": "Call Time"})
    incident_numbers = soup.findAll("td", attrs={"data-th": "Incident #"})

    #Filter out the instances we do not want
    x = 0
    indicies_to_delete = []
    while x < len(incident_instance):
        incident_instance[x] = incident_instance[x].getText().strip()
        if in_BCSO_dict(incident_instance[x]):
            incident_instance[x] = get_BCSO_dict_value(incident_instance[x])
        else:
            indicies_to_delete.append(x)
        addresses[x] = addresses[x].getText().strip()
        call_dates[x] = call_dates[x].getText().strip()
        call_times[x] = call_times[x].getText().strip()
        incident_numbers[x] = incident_numbers[x].getText().strip()
        x += 1

    temp_incidents_instance = numpy.array(incident_instance)
    temp_addresses = numpy.array(addresses)
    temp_cd = numpy.array(call_dates)
    temp_ct = numpy.array(call_times)
    temp_in = numpy.array(incident_numbers)

    incident_instance = numpy.delete(temp_incidents_instance, indicies_to_delete).tolist()
    addresses = numpy.delete(temp_addresses, indicies_to_delete).tolist()
    call_dates = numpy.delete(temp_cd, indicies_to_delete).tolist()
    call_times = numpy.delete(temp_ct, indicies_to_delete).tolist()
    incident_numbers = numpy.delete(temp_in, indicies_to_delete).tolist()


    time_slot_map = get_timeslot_map(engine)

    time_slot_ids = []
    call_datetimes = []
    for x in range(0, len(call_dates)):

        call_date = call_dates[x]
        call_time = call_times[x]

        # Combine date and time into a single string
        call_datetime_string = f"{call_date} {call_time}"

        # Convert the combined string into a datetime object
        call_datetime = datetime.datetime.strptime(call_datetime_string, '%m/%d/%Y %I:%M %p')

        call_datetimes.append(call_datetime)

        day_of_week, time_of_day = datetime_to_timeslot_id(call_datetime)

        time_slot_tuple = tuple((day_of_week, time_of_day))

        time_slot_id = time_slot_map[time_slot_tuple]

        time_slot_ids.append(time_slot_id)

    incident_wrapper = [call_datetimes, addresses, incident_instance, incident_numbers]
    num_of_incidents = len(incident_wrapper[0])

    crimes = []    
    for index in range(0, num_of_incidents):

        crime = create_crime_dict(incident_numbers[index], AGENCY_ID, time_slot_ids[index], incident_instance[index])
        crimes.append(crime)

    unique_key = ["agency_id", "incident_id"]
    return_columns = ["id", "incident_id"]

    result = insert_data(engine, crimes, Crime, return_columns=return_columns, natural_key=unique_key)

    incident_id_map = {item['incident_id']: item['id'] for item in result}

    crime_addresses = []    
    for index in range(0, num_of_incidents):

        incident_id = int(incident_numbers[index])
        crime_id = incident_id_map[incident_id]

        address = {
            "crime_id": crime_id,
            "address": addresses[index],
        }
        crime_addresses.append(address)

    unique_key = ["crime_id"]
    insert_data(engine, crime_addresses, CrimeAddress, natural_key=unique_key)
    

def collect_BSCO_crime_data(start_date, end_date, engine):

    row_count = 10000

    while start_date < end_date:

        month_ahead_date = start_date + relativedelta(months=1)

        if end_date < month_ahead_date:

            print(f"Collecting BSCO crimes between {start_date.strftime('%m/%d/%Y'),} and {end_date.strftime('%m/%d/%Y'),}")
            run_BSCO_scrape(start_date, end_date, row_count, engine)
            start_date = end_date

        else:
            print(f"Collecting BSCO crimes between {start_date.strftime('%m/%d/%Y'),} and {month_ahead_date.strftime('%m/%d/%Y'),}")
            run_BSCO_scrape(start_date, month_ahead_date, row_count, engine)
            start_date = month_ahead_date

def datetime_to_timeslot_id(crime_datetime):

    day_of_week = crime_datetime.strftime('%A')

    # Determine the time of day range
    if crime_datetime.time() < datetime.time(6, 0):  # 12am to 5:59:59am
        time_of_day = 1
    elif crime_datetime.time() < datetime.time(12, 0):  # 6am to 11:59:59am
        time_of_day = 2
    elif crime_datetime.time() < datetime.time(18, 0):  # 12pm to 5:59:59pm
        time_of_day = 3
    else:  # 6pm to 11:59:59pm
        time_of_day = 4

    return day_of_week, time_of_day

def get_timeslot_map(engine):

    query = s.text("""
        Select * from time_slots
    """)

    rows = engine.execute(query).fetchall()

    time_slot_map = {}
    for row in rows:
        row = dict(row)

        id = row["id"]
        day_of_week = row["day_of_week"]
        time_of_day = int(row["time_of_day"])
        time_slot_tuple = tuple((day_of_week, time_of_day))

        time_slot_map[time_slot_tuple] = id

    return time_slot_map
def collect_CPD_data(startDate, endDate, engine):
    session = requests.Session()
    agency_url = "https://www.como.gov/CMS/911dispatch/police.php"
    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36",
    }
    parameters = {
        'type':'',
        'keyword':'',
        'Start_Date':startDate,
        'End_Date': endDate
    }

    session.post(agency_url, headers=headers, data=parameters)

    file_url = "https://www.como.gov/CMS/911dispatch/police_csvexport.php"
    
    download = session.get(file_url)

    decoded_content = download.content.decode('utf-8')

    cr = csv.reader(decoded_content.splitlines(), delimiter=',')
    my_list = list(cr)
    del my_list[0]
    data = []

    incident_id_address_map = {}
    for incident in my_list:
        if is_violent_CPD_crime(incident[3]):
            incident_date_time = convert_CPD_time_to_datetime(incident[1])

            agency_id = 2
            time_slot_id = 1
            incident_id = int(incident[0])
            address = incident[2]
            crime_type = map_from_CPD_incident_type(incident[3])

            crime = create_crime_dict(incident_id, agency_id, time_slot_id, crime_type)
            data.append(crime)

            incident_id_address_map[incident_id] = address

            #data.append({"Incident ID": incident[0], "Type": map_from_CPD_incident_type(incident[3]), "Address": incident[2], "Date/Time of Incident": incident_date_time})
        else:
            pass

    unique_key = ["agency_id", "incident_id"]
    return_columns = ["id", "incident_id"]
    result = insert_data(engine, data, Crime, return_columns=return_columns, natural_key=unique_key)

    incident_id_map = {item['incident_id']: item['id'] for item in result}

    crime_addresses = []    
    for crime in data:

        incident_id = crime["incident_id"]

        crime_id = incident_id_map[incident_id]
        address = incident_id_address_map[incident_id]
        
        address = {
            "crime_id": crime_id,
            "address": address,
        }
        crime_addresses.append(address)

    unique_key = ["crime_id"]
    insert_data(engine, crime_addresses, CrimeAddress, natural_key=unique_key)


    session.close()
    
def is_violent_CPD_crime(incident_type):
    for key in crimedict.cpddict.keys():
        if incident_type in key:
            return True
    return False

def map_from_CPD_incident_type(incident_type):
    for key in crimedict.cpddict.keys():
        if incident_type in key:
            return crimedict.cpddict.get(key)
    return None

def convert_CPD_time_to_datetime(raw_datetime):
    matches = re.search(r"([0-9]{1,2}[/][0-9]{1,2}[/][0-9]{1,4}) ([0-9]{1,2}[:][0-9]{1,2})[:][0-9]{2}[ ]([A-Z]{2})", raw_datetime)
    date = matches.group(1)
    time = matches.group(2)
    day_cycle = matches.group(3)

    completeTime = "" + date + " " + time + " " + day_cycle
   
    datetime_of_incident = datetime.datetime.strptime(completeTime, '%m/%d/%Y %I:%M %p')
    return datetime_of_incident

def run_CPD_scrape(startDate, endDate, engine):


    row_count = 10000

    while startDate < endDate:

        month_ahead_date = startDate + relativedelta(months=1)

        if endDate < month_ahead_date:

            print(f"Collecting CPD crimes between {startDate.strftime('%m/%d/%Y'),} and {endDate.strftime('%m/%d/%Y'),}")
            run_BSCO_scrape(startDate, endDate, row_count, engine)
            startDate = endDate

        else:
            print(f"Collecting CPD crimes between {startDate.strftime('%m/%d/%Y'),} and {month_ahead_date.strftime('%m/%d/%Y'),}")
            run_BSCO_scrape(startDate, month_ahead_date, row_count, engine)
            startDate = month_ahead_date



    # #Assuming datetimes are given
    # currentDate = startDate

    # while currentDate != endDate:
    #     daysInMonth = str(calendar.monthrange(currentDate.year, currentDate.month)[1])

    #     if (currentDate.month == endDate.month) and (currentDate.year == endDate.year):

    #         print(f"Collecting CPD crimes between {currentDate.strftime('%m/%d/%Y')} and {endDate.strftime('%m/%d/%Y')}")
    #         collect_CPD_data(currentDate.strftime("%Y-%m-%d"), endDate.strftime("%Y-%m-%d"), engine)
    #         currentDate = endDate
    #     else:
    #         print(f"Collecting CPD crimes between {currentDate} and {endDate}")
    #         collect_CPD_data(currentDate.strftime("%Y-%m-01"), currentDate.strftime("%Y-%m-" + daysInMonth), engine)
    #         if currentDate.month == 12:
    #             currentDate = currentDate + relativedelta(years = 1)
    #             currentDate = currentDate.replace(month=1)
    #         else:
    #             currentDate = currentDate + relativedelta(months = 1)

                
# run_BSCO_scrape('01/01/2023', '01/31/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('02/01/2023', '02/28/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('03/01/2023', '03/31/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('04/01/2023', '04/30/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('05/01/2023', '05/31/2023', 10000)
#run_CPD_scrape("2023-06-10", "2023-06-11")


#startTest = datetime.datetime(2023, 6, 1)
#endTest = datetime.datetime(2023, 7, 1)

#run_CPD_scrape(startTest, endTest)