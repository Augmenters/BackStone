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

    #this could probably be improved to take a table as a parameter, you can do that in sql server. Not sure how to do it in postgres.
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

        con.execute(mw_update_address_hash)
        print("created procedure mw_update_address_hash")
