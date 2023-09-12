from sqlalchemy.sql import text

def create_stored_procedures(engine):
    gt_unhashed_addresses = """create or replace function gt_unhashed_addresses()
                               returns table (id bigint, latitude varchar, longitude varchar) as $$
                               begin
                                    return query
                                    select
                                    a.id,
                                    a.latitude,
                                    a.longitude
                                    from addresses as a
                                    where geohash is null ;
                               end; $$
                               language 'plpgsql';"""

    gt_time_slots = """create or replace function gt_time_slots()
                       returns table (id smallint, day_of_week varchar, time_of_day varchar) as $$
                       begin
                            return query
                            select
                            ts.id,
                            ts.day_of_week,
                            ts.time_of_day
                            from time_slots as ts;
                       end; $$
                       language 'plpgsql';"""

    gt_crime_by_timeslot_id = """create or replace function gt_crime_by_timeslot_id(timeslot_id int)
                                  returns table (grid_hash varchar, crime_count bigint) as $$
                                  begin
                                        return query
                                        select
                                        tsg.grid_hash,
                                        tsg.crime_count
                                        from time_slot_grids as tsg
                                        where tsg.time_slot_id = timeslot_id;
                                  end; $$
                                  language 'plpgsql';"""

    mw_update_address_hash = """create or replace procedure mw_update_address_hash(json)
                                language 'plpgsql' 
                                as $$
                                begin
                                    update addresses set geohash = x.hash from json_to_recordset($1) x
                                    (
                                        address_id int,
                                        hash text
                                    )
                                    where id = x.address_id;
                                end; $$"""

    with engine.connect() as con:
        con.execute(gt_unhashed_addresses)
        print("created procedure gt_unhashed_addresses")

        con.execute(gt_crime_by_timeslot_id)
        print("created procedure gt_crime_by_timeslot_id")

        con.execute(mw_update_address_hash)
        print("created procedure mw_update_address_hash")
