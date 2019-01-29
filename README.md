# TestTaskFileCompresion
Compression big file with gZip library and .net 3.5 Task

Simple file compress/decompress multithreaded algorithm based on  threads and GZipStream (.net 3.5 only)

Problems:
1)InitializationLogic. Внедрить нормальную Dep Inj.
2)BaseWriterLogic. Декомпозировать глубже. По примеру BaseReadersLogic.
3)SystemSettingMonitor. Вынести логирование.
4)StreamResultQueue. Работает, но была проблема с которой я не разобрался. 
При синхронизации чезер инстанс (Instance) падало с ошибкой связанной с изменением коллекции queue. 
Воспроизводилось только для очель больших вайлов (порядка 32г) и только при декомпрессии.
5)CancellationTokenSource. Наивная реализация подсмотренная в mscorlib :)
6)BaseReader and BaseWriterLogic. После catche в finally можно было попробовать запустить процесс еще раз.
И только после этого останавливать через token.
