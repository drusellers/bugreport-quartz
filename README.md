# How To run

In one terminal I do

```sh
docker compose up
```

then in another terminal

```sh
dotnet run
```

That should generate the error.

---

## To fix 

Uncomment out the SQL in `pg/createdbs.sh` which will create the table
