\# AR Mission Cards: Helden der Zeit



AR Mission Cards ist ein interaktives Augmented-Reality-Spiel, das mit Unity für Android entwickelt wurde.



Die Anwendung enthält drei unterschiedliche Mini-Games. Jedes Spiel wird durch das Scannen einer eigenen Marker-Karte gestartet.



\## Projektübersicht



\- \*\*Projektname:\*\* AR Mission Cards: Helden der Zeit

\- \*\*Unity-Version:\*\* Unity 6.0 / 6000.0.61f1

\- \*\*Zielplattform:\*\* Android

\- \*\*Startszene:\*\* StartScene



\## Verwendete Technologien



\- Unity

\- C#

\- AR Foundation

\- ARCore XR Plugin

\- XR Reference Image Library

\- ARTrackedImageManager

\- Unity Input System

\- Git und GitHub



\## Mini-Games



\### 1. Roman Soldier – Arena-Kampfspiel



Ein AR-Kampfspiel, in dem der Spieler gegen einen römischen Gegner kämpft.



Funktionen:



\- Normale und spezielle Angriffe

\- Verteidigungsmechanik

\- Lebensanzeigen für Spieler und Gegner

\- Gegnerbewegung und Angriffssystem

\- Animationen und Soundeffekte

\- Gewinn- und Verlustbedingungen



\### 2. Pyramid Quest – Ägyptisches Wissensspiel



Ein interaktives Quizspiel mit Fragen zum alten Ägypten.



Funktionen:



\- Zufällige Auswahl von Quizfragen

\- Drei-Leben-System

\- Richtige und falsche Antwortlogik

\- Animierte Pyramidentür

\- Gewinn- und Game-Over-System



\### 3. Great Wall Defense – Bau- und Verteidigungsspiel



Ein zeitbasiertes Bau-Spiel, in dem eine Mauer innerhalb einer begrenzten Zeit aufgebaut werden muss.



Funktionen:



\- Drag-and-Drop-Steuerung

\- Platzierung verschiedener Mauerteile

\- Zeitlimit

\- Fortschrittskontrolle

\- Soldaten- und Umgebungsanimationen

\- Gewinn- und Verlustbedingungen



\## Marker



Für jedes Mini-Game wird ein eigener Bildmarker verwendet:



| Mini-Game | Marker-Name |

|---|---|

| Roman Soldier | `roman\_soldier\_card` |

| Pyramid Quest | `Anubis\_Marker` |

| Great Wall Defense | `GreatWallMarker` |



\## Anwendung starten



1\. Das Projekt mit Unity `6000.0.61f1` öffnen.

2\. Als Zielplattform Android auswählen.

3\. Ein ARCore-kompatibles Android-Gerät verbinden.

4\. Die Szene `StartScene` öffnen.

5\. Die Anwendung erstellen und auf dem Gerät starten.

6\. Auf \*\*„Spiel starten“\*\* drücken.

7\. Eine der Marker-Karten scannen.



\## Marker zum Testen



Zum Testen müssen die Marker-Karten aus der Datei beziehungsweise dem Ordner `Markers` verwendet werden.



Nach dem Scannen eines Markers wird automatisch das zugehörige Mini-Game gestartet.



\## Projektstruktur



```text

Assets/             Szenen, Skripte, Modelle, Animationen, Audio und UI

Packages/           Unity-Pakete und Abhängigkeiten

ProjectSettings/    Unity-Projekteinstellungen

