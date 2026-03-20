# Housing and House Types Reference

This document summarizes residential plot categories, blueprint types, room/furniture support, and the property-maintenance simulation tracked on each home record.

## Core property simulation tracks
- Ownership, lot linkage, plot type, blueprint type, home type label, bedrooms, bathrooms, floor area, and furniture style.
- Daily costs including rent, mortgage, electricity, water, internet, gas, and trash.
- Living-condition scores including room quality, comfort, cleanliness, clutter, odor, lighting, decor, noise, and neighborhood desirability.
- Utility usage, storage usage, appliance condition, laundry state, trash, dishes, and repair requests.

## Plot types
- **StandardLot** — lot-level residential parcel archetype used when deriving home blueprints and neighborhood feel.
- **CornerLot** — lot-level residential parcel archetype used when deriving home blueprints and neighborhood feel.
- **RuralHomestead** — lot-level residential parcel archetype used when deriving home blueprints and neighborhood feel.
- **WaterfrontParcel** — lot-level residential parcel archetype used when deriving home blueprints and neighborhood feel.
- **CompactUrbanLot** — lot-level residential parcel archetype used when deriving home blueprints and neighborhood feel.
- **EstateParcel** — lot-level residential parcel archetype used when deriving home blueprints and neighborhood feel.

## House blueprint types
- **CompactStarter** — seeded structural archetype used to derive bedrooms, bathrooms, floor area, furniture layout, and cost curves.
- **FamilyRanch** — seeded structural archetype used to derive bedrooms, bathrooms, floor area, furniture layout, and cost curves.
- **UrbanTownhouse** — seeded structural archetype used to derive bedrooms, bathrooms, floor area, furniture layout, and cost curves.
- **RuralFarmhouse** — seeded structural archetype used to derive bedrooms, bathrooms, floor area, furniture layout, and cost curves.
- **WaterfrontDuplex** — seeded structural archetype used to derive bedrooms, bathrooms, floor area, furniture layout, and cost curves.
- **EstateManor** — seeded structural archetype used to derive bedrooms, bathrooms, floor area, furniture layout, and cost curves.

## Furniture categories
- **Seating** — placement bucket used by generated furniture layout records.
- **Sleeping** — placement bucket used by generated furniture layout records.
- **Storage** — placement bucket used by generated furniture layout records.
- **Dining** — placement bucket used by generated furniture layout records.
- **Bath** — placement bucket used by generated furniture layout records.
- **Kitchen** — placement bucket used by generated furniture layout records.
- **Decor** — placement bucket used by generated furniture layout records.
- **Utility** — placement bucket used by generated furniture layout records.

## Waste states
- **Fresh** — disposal/material state used when trash or recycling is registered against a property.
- **Used** — disposal/material state used when trash or recycling is registered against a property.
- **Waste** — disposal/material state used when trash or recycling is registered against a property.
- **Recyclable** — disposal/material state used when trash or recycling is registered against a property.
- **Hazardous** — disposal/material state used when trash or recycling is registered against a property.

## Laundry states
- **Clean** — current laundry processing state stored on the property record.
- **Dirty** — current laundry processing state stored on the property record.
- **Wet** — current laundry processing state stored on the property record.
- **Drying** — current laundry processing state stored on the property record.
