
Protocol
Technical steps:
 - Designs
   - Ich habe eine Klasse mit dem Namen „Server“, in der alle Requests gehandled werden (in der Funktion „RequestHandler(TpcClient client)“).
In dieser Funktion wird der Request in url (z.B „/users), method („GET“/ „PUT“/„POST“), token(Authorization), userToken (username in token) und body gesplittet.
Mithilfe dieser Unterteilung des Requests kann in der Funktion mit If-Statements auf die passenden Zeilen im Curl-Script zugegriffen werden.

   - In der Klasse „User“ wird jedem User ein Username, ein Password, ein Name, eine Bio und ein Image zugewiesen.
   - In der Klasse „Card“ wird jeder Card eine Id, ein Name und ein damage zugewiesen.

    - In der Klasse „CardHandler“ wird eine Liste aus packages, welche aus je 5 Karten bestehen, erstellt. Diese packages werden in die Datenbank mit dem entsprechenden foreign-key (= userid) gespeichert (-> Funktion in Klasse „Interaction“). Es gibt ebenfalls eine Liste aus Cards (boughtcard), worin alle gekauften Cards vom jeweiligen User gespeichert sind. Diese Cards werden ebenfalls in die Datenbank mit dem entsprechenden foreign-key (userid) gespeichert.

   - In der Klasse „Trade“ wird jedem Trade seine Id, die Id der Card, die im Trade benutzt wird, der Type, die minimum Damage und der username zugewiesen.

   - In der Klasse „Interaction“ interagiert der Server mit den Klassen, indem sinnvolle Funktionen in der Interaction Klasse geschrieben werden und im Server aufgerufen werden.

   - In der Klasse „Arena“ findet das Battle statt.

   - In den Klassen „TestInteraction“ und „TestArena“ befinden sich die Unit Tests.

 - Lessons learned 
   - C# basics
   - Handle and parse HTTP requests
   - To choose helpful, necessary Unit-Tests
   - How to handle a Postgres – Database
   - Client – Server communication
   - Query handling

 - Unit Test decisions:
   - Die Unit-Tests testen überwiegend die Interaction-Klasse, die für die Datenbankverbindungen zuständig ist. Datenbankabfragen sind sehr wichtig und fatal für die Funktionalität des Programms.
 - Unique features:
   - Reinkarnation
Sollte jeder Benutzer im Kampf nur mehr eine Karte besitzen und das Match unentschieden ausfallen, dann bekommt jeder Spieler eine zufällige tote Karte ins Deck.
Karten, die im Kampf sterben, werden zwischengespeichert und eventuell wieder zum Leben erweckt.
 - Tracked time (in hours):
   - Expected:  ~ 60 hours
   - Reality:  ~ 45 hours
 - Link to GIT:
   - https://github.com/amira2amy/MCTG_1

