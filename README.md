# TestTaskFileCompresion
Compression big file with gZip library and .net 3.5 Threads

Simple file compress/decompress multithreaded algorithm based on  threads and GZipStream (.net 3.5 only)

Problems:

1)StreamResultQueue. Работает, но синхронизация сложна для понимания. Возможно можно лучше, но это не точно.

2)BaseReaderLogic. Слишком большой блок стоит в lock. Возможно можно лучше, но это не точно.
