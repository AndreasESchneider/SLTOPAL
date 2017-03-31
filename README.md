# SPORTLOOP TOPAL Client

Der "SPORTLOOP TOPAL Client" übernimmt Rechnungen, die in SPORTLOOP erstellt worden sind, in die TOPAL Finanzbuchhaltung oder aktualisiert den Zahlungsstatus der Rechnung zurück nach SPORTLOOP.



## Lösung

Die Lösung wurde mit Visual Studio 2015 unter C# erstellt und benötigt zudem:

* eine Referenz auf TOPAL SAPI.dll und TOPAL Types.dll und natürlich eine TOPAL Finanzbuchhaltung. Eine ausführliche Beschreibung der TOPAL API sowie Beispiele findet sich unter <http://developer.topal.ch/Topal_API_Description/index.htm>

* zwei Queues in Azure Servicebus. Die *In* Queue wird für den Import der Rechnungen, die *Out* Queue für die Aktualisierung vom Zahlungsstatus der Rechnung verwendet. Eine Beschreibung vom Azure ServiceBus findet sich unter <https://azure.microsoft.com/en-us/services/service-bus/>



## Projektbeschreibung

Die Lösung kann vielleicht als Beispiel für andere Anwendungsfälle dienen und besteht aus den folgenden Projekten:

* **slClientBus** das die Kommunikation mit der Azure ServiceBus Queue übernimmt. Im Wesentlichen der Empfang und den Versand von Meldungen über den ServiceBus
 
* **slToTopalClient** das die Kommunikation mit TOPAL übernimmt und eine Abstraktion für die GUI bietet
 
* **slToTopalGui** für das Benutzerinterface und der eigentliche "Arbeiter" im Prozess
 
* **slToTopalModel** als Modell für den Austausch der Daten zwischen TOPAL und SPORTLOOP

Zur Vereinfachung haben wir Aspekte (PostSharp), das originale Logging (Gibraltar LOUPE) sowie die Unit Tests (MSTEST, JustMock und NCRUNCH) sowie das Self-Updating aus der Lösung entfernt. 

Aus Lizenz- und Kostengründen haben wir für diesen Client nicht NServiceBus verwendet sondern greifen direkt über die Microsoft library auf den ServiceBus zu.



## Warum mit ServiceBus?

Wir verwenden innerhalb von SPORTLOOP eine REST API und den ServiceBus für die Prozesse. Der Grund warum wir nicht die REST API für den Austausch der Daten mit TOPAL verwendet haben wie folgt:

* Der ServiceBus vereinfacht die Bearbeitung der Rechnungen insbesondere das Locking und die Quittierung
 
* Netzwerkprobleme, Unterbrüche, Abbrüche, Abstürze etc. sind einfacher zu behandeln da der ServiceBus Konsistenz garantiert

* Der GUI Client (oder ggf. als Service) kann mehrfach ausgeführt werden ohne das sich durch die gleichzeitige Ausführung Probleme ergeben


Die Wahl für den ServiceBus und gegen die REST API erfolgte weil damit die Schnittstelle inhärent sicherer und zuverlässiger wird. Nachteil ist die Abhängigkeit vom ServiceBus. Da wir diesen sowieso für alle Prozesse in SPORTLOOP verwenden war dies für uns nicht relevant.


## Dankeschön an TOPAL

An dieser Stelle vielen Dank an das TOPAL Team und an Herrn Fabiano Montagnin für die tolle Unterstützung und für die TOPAL Testlizenz. Wir können die Zusammenarbeit mit TOPAL nur empfehlen <http://www.topal.ch>.


