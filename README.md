# Sqlite Guid Spike

Sqlite can store Guids in two ways: as strings or as blobs (byte arrays). This small program tests insert, read, and storage costs of each approach.

Sqlite can insert binary as hexidecimal strings with `X''` strings, e.g. `select X'FFAA11';`, and convert binary to hex strings with `quote()`, e.g. `SELECT quote(myBlobColumn);`. This is valid: `SELECT quote(X'FFAA11');`. Unfortunately, due to endianness (I think) of Guids, you cannot use these functions to turn bytes into human-readable guids, and Sqlite has no built-in functions to create human-readable guids.

Some Powershell to illustrate the point:
```
> [Guid]::NewGuid().ToString("N")
83735fe881dd41399960d9182e24525f

> [Guid]("83735fe881dd41399960d9182e24525f")

Guid
----
83735fe8-81dd-4139-9960-d9182e24525f

> [Convert]::ToHexString(([Guid]("83735fe881dd41399960d9182e24525f")).ToByteArray())     
E85F7383DD8139419960D9182E24525F
```

Parts of the HexString match the Guid (final two couplets), others are out of sequence (first three couplets). 

## Pros and Cons of Blobs and Text

The cost difference of inserting and retrieving between the two formats is negligible. The true differences are that storing text takes twice as many bytes on disk vs bytes, and there is risk that you could insert the same text-based Guid with different casing; a trigger could solve this to force all inserts and updates to uppercase. 

Performance of inserting and then selecting 1,000,000 rows. 

```
Insert Blob: 00:11:20.0547050
Select Blob: 00:00:07.3867385
Insert Text: 00:11:05.9449975
Select Text: 00:00:08.6372038
```

| |Blob|Text|
|-|----|----|
|Human readable|No|Yes|
|Storage cost|1X|2X|
|Insert cost|Same|Same|
|Read cost|Same|Same|
|Case-sensitive|No|Yes|