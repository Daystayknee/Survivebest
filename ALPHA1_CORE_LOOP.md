# Alpha 1 Core Loop

## Player Loop (Single Day)
1. **Start Day**
   - Load or initialize run seed/profile/context.
   - Clock/day slice/weather initialize and publish startup events.
2. **Assess State**
   - Check needs, health/status, funds, pantry, pending orders/contracts.
3. **Plan Actions**
   - Buy groceries / cook / work / rest / social / treat conditions.
4. **Execute**
   - Actions modify inventory/economy/needs and emit events.
   - NPCs advance schedules and town service availability updates.
5. **React to Emergence**
   - Story incidents + AI director beats + contracts/opportunities.
   - Crime/justice/substance/medical side effects may escalate.
6. **End-of-Day Resolution**
   - Day slice transition, payouts/bills/fines/order deliveries/status ticks.
   - Journal feed and HUD summaries update.
7. **Persist**
   - Save snapshot with migration-safe schema.

## Success Conditions
- Player survives and keeps core needs from collapse.
- Economy remains solvent enough to continue.
- Consequences (social/legal/medical/town) remain understandable via feed/events.
