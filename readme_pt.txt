================================================================================
  QR TO ZIP REBUILDER
  Guia do utilizador (Português)
================================================================================

VISÃO GERAL
-----------
Esta aplicação reconstrói um ficheiro ZIP a partir de um conjunto de fotos em
cada qual aparece um código QR. Serve para o caso de um ficheiro ter sido
dividido em muitas imagens QR: cada um transporta um fragmento de texto em
Base64; o programa lê todos os fragmentos na ordem correta, junta-os,
descodifica os dados e grava o ficheiro recuperado no disco.

O QUE O PROGRAMA FAZ
--------------------
  • Escolhe a pasta que contém as imagens (por exemplo PNG, JPG).
  • A lista de ficheiros é apresentada numa grelha, ordenada de forma
    previsível com base nos algarismos do nome (ex.: imagem_1 antes de
    imagem_10).
  • Ao iniciar a reconstrução, cada imagem é tratada: o QR é detetado e o
    respetivo texto é lido. O progresso e o estado de cada ficheiro aparecem
    num registo.
  • Se todas as leituras forem bem sucedidas, a Base64 concatenada é
    convertida de volta em dados binários e guardada como
    "codigo_recuperado.zip" na mesma pasta que selecionou.
  • Se alguma leitura falhar, o arquivo não é gerado, para não produzir um
    ficheiro incompleto: obtém-se ou um resultado completo, ou erros
    explícitos.

UTILIZAÇÃO TÍPICA
-----------------
Fotografe a sequência de códigos QR (por exemplo no ecrã ou numa impressão),
 coloque as fotos numa pasta, abra o programa, indique essa pasta e execute
a reconstrução. Mantenha a câmara estável, evite reflexos e assegure que
cada QR se vê completo e nítido.

REQUISITOS
----------
  • Microsoft Windows
  • .NET 8 (runtime) ou, para compilar, o SDK .NET 8

================================================================================
