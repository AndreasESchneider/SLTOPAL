# SPORTLOOP TOPAL Client

Der "SPORTLOOP TOPAL Client" �bernimmt Rechnungen, die in SPORTLOOP erstellt worden sind, in die TOPAL Finanzbuchhaltung oder aktualisiert den Zahlungsstatus der Rechnung zur�ck nach SPORTLOOP.



## L�sung

Die L�sung wurde mit Visual Studio 2015 unter C# erstellt und ben�tigt zudem:

* eine Referenz auf TOPAL SAPI.dll und TOPAL Types.dll und nat�rlich eine TOPAL Finanzbuchhaltung. Eine ausf�hrliche Beschreibung der TOPAL API sowie Beispiele findet sich unter <http://developer.topal.ch/Topal_API_Description/index.htm>

* zwei Queues in Azure Servicebus. Die *In* Queue wird f�r den Import der Rechnungen, die *Out* Queue f�r die Aktualisierung vom Zahlungsstatus der Rechnung verwendet. Eine Beschreibung vom Azure ServiceBus findet sich unter <https://azure.microsoft.com/en-us/services/service-bus/>



## Projektbeschreibung

Die L�sung kann vielleicht als Beispiel f�r andere Anwendungsf�lle dienen und besteht aus den folgenden Projekten:

* **slClientBus** das die Kommunikation mit der Azure ServiceBus Queue �bernimmt. Im Wesentlichen der Empfang und den Versand von Meldungen �ber den ServiceBus
 
* **slToTopalClient** das die Kommunikation mit TOPAL �bernimmt und eine Abstraktion f�r die GUI bietet
 
* **slToTopalGui** f�r das Benutzerinterface und der eigentliche "Arbeiter" im Prozess
 
* **slToTopalModel** als Modell f�r den Austausch der Daten zwischen TOPAL und SPORTLOOP

Zur Vereinfachung haben wir Aspekte (PostSharp), das originale Logging (Gibraltar LOUPE) sowie die Unit Tests (MSTEST, JustMock und NCRUNCH) sowie das Self-Updating aus der L�sung entfernt. 

Aus Lizenz- und Kostengr�nden haben wir f�r diesen Client nicht NServiceBus verwendet sondern greifen direkt �ber die Microsoft library auf den ServiceBus zu.



## Warum mit ServiceBus?

Wir verwenden innerhalb von SPORTLOOP eine REST API und den ServiceBus f�r die Prozesse. Der Grund warum wir nicht die REST API f�r den Austausch der Daten mit TOPAL verwendet haben wie folgt:

* Der ServiceBus vereinfacht die Bearbeitung der Rechnungen insbesondere das Locking und die Quittierung
 
* Netzwerkprobleme, Unterbr�che, Abbr�che, Abst�rze etc. sind einfacher zu behandeln da der ServiceBus Konsistenz garantiert

* Der GUI Client (oder ggf. als Service) kann mehrfach ausgef�hrt werden ohne das sich durch die gleichzeitige Ausf�hrung Probleme ergeben


Die Wahl f�r den ServiceBus und gegen die REST API erfolgte weil damit die Schnittstelle inh�rent sicherer und zuverl�ssiger wird. Nachteil ist die Abh�ngigkeit vom ServiceBus. Da wir diesen sowieso f�r alle Prozesse in SPORTLOOP verwenden war dies f�r uns nicht relevant.


## Dankesch�n an TOPAL

An dieser Stelle vielen Dank an das TOPAL Team und an Herrn Fabiano Montagnin f�r die tolle Unterst�tzung und f�r die TOPAL Testlizenz. Wir k�nnen die Zusammenarbeit mit TOPAL nur empfehlen <http://www.topal.ch>.


