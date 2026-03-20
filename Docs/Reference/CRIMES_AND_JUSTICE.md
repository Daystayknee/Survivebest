# Crimes and Justice Reference

This document describes the crime catalog and the main justice-facing simulation assumptions exposed by `CrimeSystem` and `LawSystem`.

## Crime simulation rules
- Every crime resolves into a crime category and a law severity band.
- Witnesses, evidence, area enforcement, and police presence feed police-awareness and arrest-chance calculations.
- Crimes can resolve immediately into justice processing or open a delayed investigation that ticks down by hour.
- Substance legality is area-specific and can be changed by law votes.

## Crime categories
- **MinorOffense** — resolution bucket used for enforcement tuning and downstream justice logic.
- **SubstanceCrime** — resolution bucket used for enforcement tuning and downstream justice logic.
- **ViolentCrime** — resolution bucket used for enforcement tuning and downstream justice logic.
- **PropertyCrime** — resolution bucket used for enforcement tuning and downstream justice logic.
- **PublicDisorder** — resolution bucket used for enforcement tuning and downstream justice logic.
- **PoliticalCrime** — resolution bucket used for enforcement tuning and downstream justice logic.

## Crime types
- **Theft** — explicit commit-able offense type exposed by the current crime system.
- **Assault** — explicit commit-able offense type exposed by the current crime system.
- **Vandalism** — explicit commit-able offense type exposed by the current crime system.
- **Trespassing** — explicit commit-able offense type exposed by the current crime system.
- **DrugPossession** — explicit commit-able offense type exposed by the current crime system.
- **DrugDistribution** — explicit commit-able offense type exposed by the current crime system.
- **Burglary** — explicit commit-able offense type exposed by the current crime system.
- **Fraud** — explicit commit-able offense type exposed by the current crime system.
- **PublicDisorder** — explicit commit-able offense type exposed by the current crime system.
- **ElectionFraud** — explicit commit-able offense type exposed by the current crime system.

## Law severity ladder
- **Legal** — no criminal prohibition.
- **Infraction** — lowest illegal band; small but non-zero legal consequences.
- **Misdemeanor** — medium illegal band; meaningful arrest and punishment risk.
- **Felony** — highest illegal band in the current law model.
