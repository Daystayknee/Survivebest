#!/usr/bin/env python3
"""Lightweight smoke tests for autonomy/social decision math.

These checks intentionally mirror the formulas in:
- Assets/Scripts/NPC/AutonomyDecisionEngine.cs
- Assets/Scripts/NPC/NpcSocialInteractionModel.cs

They are executable in environments without Unity/.NET and give us a
repeatable safety net for core tuning values.
"""

from __future__ import annotations


def clamp(value: float, minimum: float, maximum: float) -> float:
    return max(minimum, min(maximum, value))


def inverse_lerp(a: float, b: float, value: float) -> float:
    if a == b:
        return 0.0
    return clamp((value - a) / (b - a), 0.0, 1.0)


def compute_social_drive(
    loneliness: float,
    social_battery: float,
    memory_sentiment: float,
    relationship_affinity: float,
    stress: float,
) -> float:
    loneliness_pressure = inverse_lerp(20.0, 90.0, loneliness)
    battery_readiness = inverse_lerp(10.0, 85.0, social_battery)
    memory_bias = inverse_lerp(-100.0, 100.0, memory_sentiment)
    affinity_bias = inverse_lerp(-100.0, 100.0, relationship_affinity)
    stress_penalty = inverse_lerp(45.0, 95.0, stress)

    value = (
        loneliness_pressure * 0.38
        + battery_readiness * 0.22
        + memory_bias * 0.20
        + affinity_bias * 0.20
        - stress_penalty * 0.28
    )
    return clamp(value, 0.0, 1.0)


def compute_relationship_delta(
    social_drive: float,
    stress: float,
    mood: float,
    memory_sentiment: float,
    relationship_affinity: float,
) -> int:
    social_factor = 1.0 + (8.0 - 1.0) * clamp(social_drive, 0.0, 1.0)
    mood_factor = -3.0 + (3.0 - -3.0) * inverse_lerp(20.0, 80.0, mood)
    stress_penalty = 0.0 + (6.0 - 0.0) * inverse_lerp(40.0, 95.0, stress)
    memory_factor = -4.0 + (4.0 - -4.0) * inverse_lerp(-100.0, 100.0, memory_sentiment)
    affinity_factor = -3.0 + (3.0 - -3.0) * inverse_lerp(-100.0, 100.0, relationship_affinity)
    total = social_factor + mood_factor + memory_factor + affinity_factor - stress_penalty
    return round(clamp(total, -12.0, 12.0))


def test_social_drive_increases_with_positive_context() -> None:
    baseline = compute_social_drive(40, 40, 0, 0, 55)
    improved = compute_social_drive(80, 80, 80, 70, 20)
    assert improved > baseline, (baseline, improved)


def test_social_drive_reduces_with_high_stress() -> None:
    calm = compute_social_drive(75, 70, 60, 50, 20)
    overloaded = compute_social_drive(75, 70, 60, 50, 95)
    assert overloaded < calm, (calm, overloaded)


def test_relationship_delta_signals_good_and_bad_beats() -> None:
    positive = compute_relationship_delta(0.9, 20, 80, 80, 65)
    negative = compute_relationship_delta(0.2, 92, 25, -85, -70)
    assert positive > 0, positive
    assert negative < 0, negative


def run_all() -> None:
    test_social_drive_increases_with_positive_context()
    test_social_drive_reduces_with_high_stress()
    test_relationship_delta_signals_good_and_bad_beats()


if __name__ == "__main__":
    run_all()
    print("npc_logic_smoke_tests: PASS")
