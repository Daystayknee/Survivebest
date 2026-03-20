using System;
using System.Collections.Generic;
using UnityEngine;
using Survivebest.Core;

namespace Survivebest.Catalog
{
    public enum SupplyGroup
    {
        Spice,
        NutSeed,
        Legume,
        Snack,
        Dessert,
        Medicine,
        Animal,
        Skill,
        Facility,
        Object,
        Accessory,
        Clothing,
        Store,
        Foliage,
        Pet,
        Consumable,
        Electronics,
        Household,
        Trinket,
        Toy,
        Weapon,
        Tool,
        Hygiene
    }

    [Serializable]
    public class SupplyItem
    {
        public string Name;
        public SupplyGroup Group;
        public string Species;
        public string Breed;

        public string SpeciesOrName => string.IsNullOrWhiteSpace(Species) ? Name : Species;
        public bool HasBreed => !string.IsNullOrWhiteSpace(Breed);
        public string DisplayLabel => HasBreed ? $"{SpeciesOrName} ({Breed})" : Name;
    }

    public class SupplyCatalog : MonoBehaviour
    {
        [SerializeField] private List<SupplyItem> supplies = new()
        {
            // Spices & Seasonings (35)
            new SupplyItem { Name = "Black pepper", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Salt", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "White pepper", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Paprika", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Smoked paprika", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Cayenne", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Chili powder", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Turmeric", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Cumin", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Coriander", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Cardamom", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Cloves", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Cinnamon", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Nutmeg", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Mace", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Ginger powder", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Garlic powder", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Onion powder", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Mustard seed", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Fenugreek", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Star anise", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Saffron", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Sumac", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Za’atar", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Curry powder", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Garam masala", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Fennel seed", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Dill seed", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Caraway", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Bay leaf", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Oregano", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Thyme", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Basil", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Rosemary", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Sage", Group = SupplyGroup.Spice },
            new SupplyItem { Name = "Tarragon", Group = SupplyGroup.Spice },

            // Nuts & Seeds (35)
            new SupplyItem { Name = "Almond", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Walnut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Cashew", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Pistachio", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Macadamia", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Brazil nut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Pecan", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Hazelnut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Pine nut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Tiger nut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Kola nut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Pili nut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Marcona almond", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Candlenut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Baruka nut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Gingko nut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Beech nut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Chestnut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Water chestnut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Sacha inchi", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Peanuts", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Soy nuts", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Pumpkin seeds", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Sunflower seeds", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Sesame seeds", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Chia seeds", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Flax seeds", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Hemp seeds", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Watermelon seeds", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Poppy seeds", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Lotus seeds", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Breadnut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Monkey nut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Nangai nut", Group = SupplyGroup.NutSeed },
            new SupplyItem { Name = "Okari nut", Group = SupplyGroup.NutSeed },

            // Legumes (35)
            new SupplyItem { Name = "Lentils", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Chickpeas", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Black beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Kidney beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Pinto beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Navy beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Cannellini beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Great northern beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Lima beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Fava beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Mung beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Adzuki beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Soybeans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Edamame", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Split peas", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Green peas", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Black-eyed peas", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Pigeon peas", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Lupin beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Tepary beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Cranberry beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Anasazi beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Scarlet runner beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Winged beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Yardlong beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Hyacinth beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Jack beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Velvet beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Horse gram", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Moth beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Bambara groundnut", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Cowpeas", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Broad beans", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Chickling peas", Group = SupplyGroup.Legume },
            new SupplyItem { Name = "Rice beans", Group = SupplyGroup.Legume },

            // Snacks (35)
            new SupplyItem { Name = "Potato chips", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Tortilla chips", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Popcorn", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Pretzels", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Crackers", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Cheese puffs", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Nachos", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Trail mix", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Beef jerky", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Rice cakes", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Granola bars", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Protein bars", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Pita chips", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Veggie chips", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Kale chips", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Roasted chickpeas", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Corn nuts", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Fruit snacks", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Yogurt cups", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "String cheese", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Pickles", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Olives", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Chocolate bars", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Candy bars", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Gummies", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Caramel popcorn", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Donut holes", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Mini sandwiches", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Nut clusters", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Peanut butter crackers", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Rice crackers", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Seaweed snacks", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Apple chips", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Banana chips", Group = SupplyGroup.Snack },
            new SupplyItem { Name = "Energy bites", Group = SupplyGroup.Snack },

            // Desserts (35)
            new SupplyItem { Name = "Chocolate cake", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Vanilla cake", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Cheesecake", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Brownies", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Cookies", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Cupcakes", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Ice cream", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Gelato", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Sorbet", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Tiramisu", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Panna cotta", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Crème brûlée", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Apple pie", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Pumpkin pie", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Lemon tart", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Fruit tart", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Macarons", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Éclairs", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Cannoli", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Baklava", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Rice pudding", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Bread pudding", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Custard", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Chocolate mousse", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Parfait", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Donuts", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Funnel cake", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Churros", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Shortcake", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Truffles", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Fudge", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Pavlova", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Sticky toffee pudding", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Tres leches cake", Group = SupplyGroup.Dessert },
            new SupplyItem { Name = "Mochi", Group = SupplyGroup.Dessert },

            // Medicines (35)
            new SupplyItem { Name = "Acetaminophen", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Ibuprofen", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Aspirin", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Naproxen", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Amoxicillin", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Azithromycin", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Ciprofloxacin", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Doxycycline", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Penicillin", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Metronidazole", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Loratadine", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Diphenhydramine", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Cetirizine", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Pseudoephedrine", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Guaifenesin", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Dextromethorphan", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Loperamide", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Bismuth subsalicylate", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Omeprazole", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Famotidine", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Antacids", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Hydrocortisone cream", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Clotrimazole", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Antiseptic alcohol", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Hydrogen peroxide", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Iodine solution", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Insulin", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Metformin", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Albuterol inhaler", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Epinephrine", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Melatonin", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Activated charcoal", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Oral rehydration salts", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Zinc supplements", Group = SupplyGroup.Medicine },
            new SupplyItem { Name = "Vitamin C", Group = SupplyGroup.Medicine },

            // Animals (species + breed variants)
            CreateAnimal("Dog"),
            CreateAnimal("Cat"),
            CreateAnimal("Horse"),
            CreateAnimal("Cow"),
            CreateAnimalBreed("Cow", "Holstein"),
            CreateAnimalBreed("Cow", "Jersey"),
            CreateAnimalBreed("Cow", "Angus"),
            CreateAnimalBreed("Cow", "Highland"),
            CreateAnimal("Pig"),
            CreateAnimalBreed("Pig", "Yorkshire"),
            CreateAnimalBreed("Pig", "Berkshire"),
            CreateAnimal("Sheep"),
            CreateAnimalBreed("Sheep", "Merino"),
            CreateAnimalBreed("Sheep", "Suffolk"),
            CreateAnimal("Goat"),
            CreateAnimalBreed("Goat", "Nubian"),
            CreateAnimalBreed("Goat", "Boer"),
            CreateAnimal("Rabbit"),
            CreateAnimalBreed("Rabbit", "Holland Lop"),
            CreateAnimalBreed("Rabbit", "Flemish Giant"),
            CreateAnimal("Rat"),
            CreateAnimal("Mouse"),
            CreateAnimal("Squirrel"),
            CreateAnimal("Deer"),
            CreateAnimal("Bear"),
            CreateAnimal("Wolf"),
            CreateAnimal("Fox"),
            CreateAnimal("Lion"),
            CreateAnimal("Tiger"),
            CreateAnimal("Elephant"),
            CreateAnimal("Giraffe"),
            CreateAnimal("Zebra"),
            CreateAnimal("Kangaroo"),
            CreateAnimal("Koala"),
            CreateAnimal("Panda"),
            CreateAnimal("Monkey"),
            CreateAnimal("Gorilla"),
            CreateAnimal("Chimpanzee"),
            CreateAnimal("Dolphin"),
            CreateAnimal("Whale"),
            CreateAnimal("Shark"),
            CreateAnimal("Eagle"),
            CreateAnimal("Owl"),
            CreateAnimal("Hawk"),
            CreateAnimal("Falcon"),
            CreateAnimal("Penguin"),
            CreateAnimal("Flamingo"),

            // Skills (35)
            new SupplyItem { Name = "Cooking", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Gardening", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Hunting", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Fishing", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Farming", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Foraging", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Carpentry", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Blacksmithing", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Sewing", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Tailoring", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Leatherworking", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Pottery", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Painting", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Drawing", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Sculpting", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Writing", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Storytelling", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Programming", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Game design", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "UI/UX design", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Photography", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Videography", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Music composition", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Singing", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Instrument playing", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Negotiation", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Leadership", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Public speaking", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "First aid", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Survival skills", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Navigation", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Animal care", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Herbal medicine", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Chemistry", Group = SupplyGroup.Skill },
            new SupplyItem { Name = "Engineering", Group = SupplyGroup.Skill },

            // Facilities & Food Infrastructure
            new SupplyItem { Name = "Family Farm", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Industrial Farm", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Greenhouse Complex", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Orchard", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Vineyard", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Fish Hatchery", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Fishing Dock", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Dairy Plant", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Meat Processing Plant", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Slaughterhouse", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Cold Storage Warehouse", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Grain Mill", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Bakery Factory", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Beverage Bottling Plant", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Distribution Center", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Food Market", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Butcher Shop", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Seafood Market", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "Petting Zoo", Group = SupplyGroup.Facility },
            new SupplyItem { Name = "City Zoo", Group = SupplyGroup.Facility }
        };

        private void Awake()
        {
            EnsureWorldEssentials();
        }

        public bool HasSupply(string name, SupplyGroup group)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return supplies.Exists(s => s != null && s.Group == group && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private void EnsureWorldEssentials()
        {
            AddIfMissing("Backpack", SupplyGroup.Accessory);
            AddIfMissing("Watch", SupplyGroup.Accessory);
            AddIfMissing("Necklace", SupplyGroup.Accessory);
            AddIfMissing("Sunglasses", SupplyGroup.Accessory);
            AddIfMissing("Hat", SupplyGroup.Accessory);

            AddIfMissing("Work Uniform", SupplyGroup.Clothing);
            AddIfMissing("Doctor Scrubs", SupplyGroup.Clothing);
            AddIfMissing("Chef Jacket", SupplyGroup.Clothing);
            AddIfMissing("Rain Jacket", SupplyGroup.Clothing);
            AddIfMissing("Winter Boots", SupplyGroup.Clothing);

            AddIfMissing("Dining Table", SupplyGroup.Object);
            AddIfMissing("Bookshelf", SupplyGroup.Object);
            AddIfMissing("Refrigerator", SupplyGroup.Object);
            AddIfMissing("Toolbox", SupplyGroup.Object);
            AddIfMissing("Pet Bed", SupplyGroup.Object);

            AddIfMissing("General Store", SupplyGroup.Store);
            AddIfMissing("Clothing Boutique", SupplyGroup.Store);
            AddIfMissing("Hardware Store", SupplyGroup.Store);
            AddIfMissing("Pet Supply Store", SupplyGroup.Store);
            AddIfMissing("Garden Center", SupplyGroup.Store);

            AddIfMissing("Oak Tree", SupplyGroup.Foliage);
            AddIfMissing("Pine Tree", SupplyGroup.Foliage);
            AddIfMissing("Rose Bush", SupplyGroup.Foliage);
            AddIfMissing("Lavender", SupplyGroup.Foliage);
            AddIfMissing("Fern", SupplyGroup.Foliage);

            AddIfMissing("Dog", SupplyGroup.Pet);
            AddIfMissing("Cat", SupplyGroup.Pet);
            AddIfMissing("Rabbit", SupplyGroup.Pet);
            AddIfMissing("Parrot", SupplyGroup.Pet);
            AddIfMissing("Turtle", SupplyGroup.Pet);

            AddIfMissing("Pet Food", SupplyGroup.Consumable);
            AddIfMissing("Bandage", SupplyGroup.Consumable);
            AddIfMissing("Batteries", SupplyGroup.Consumable);
            AddIfMissing("Laundry Detergent", SupplyGroup.Consumable);
            AddIfMissing("Soap", SupplyGroup.Consumable);

            EnsureAmericanRetailCoverage();
        }

        private void EnsureAmericanRetailCoverage()
        {
            AddIfMissing("Laptop", SupplyGroup.Electronics);
            AddIfMissing("Tablet", SupplyGroup.Electronics);
            AddIfMissing("Smartphone", SupplyGroup.Electronics);
            AddIfMissing("Phone Charger", SupplyGroup.Electronics);
            AddIfMissing("Headphones", SupplyGroup.Electronics);
            AddIfMissing("Bluetooth Speaker", SupplyGroup.Electronics);
            AddIfMissing("Game Console", SupplyGroup.Electronics);
            AddIfMissing("TV Remote", SupplyGroup.Electronics);
            AddIfMissing("Power Bank", SupplyGroup.Electronics);
            AddIfMissing("USB Cable", SupplyGroup.Electronics);

            AddIfMissing("Paper Towels", SupplyGroup.Household);
            AddIfMissing("Trash Bags", SupplyGroup.Household);
            AddIfMissing("Dish Soap", SupplyGroup.Household);
            AddIfMissing("Light Bulbs", SupplyGroup.Household);
            AddIfMissing("Storage Bin", SupplyGroup.Household);
            AddIfMissing("Extension Cord", SupplyGroup.Household);
            AddIfMissing("Flashlight", SupplyGroup.Household);
            AddIfMissing("Cooler", SupplyGroup.Household);
            AddIfMissing("Air Mattress", SupplyGroup.Household);

            AddIfMissing("Keychain", SupplyGroup.Trinket);
            AddIfMissing("Snow Globe", SupplyGroup.Trinket);
            AddIfMissing("Souvenir Magnet", SupplyGroup.Trinket);
            AddIfMissing("Pocket Watch", SupplyGroup.Trinket);
            AddIfMissing("Lucky Coin", SupplyGroup.Trinket);
            AddIfMissing("Collectible Pin", SupplyGroup.Trinket);

            AddIfMissing("Teddy Bear", SupplyGroup.Toy);
            AddIfMissing("Action Figure", SupplyGroup.Toy);
            AddIfMissing("Toy Car", SupplyGroup.Toy);
            AddIfMissing("Building Blocks", SupplyGroup.Toy);
            AddIfMissing("Board Game", SupplyGroup.Toy);
            AddIfMissing("Basketball", SupplyGroup.Toy);
            AddIfMissing("Baseball Glove", SupplyGroup.Toy);
            AddIfMissing("Puzzle Box", SupplyGroup.Toy);

            AddIfMissing("Kitchen Knife", SupplyGroup.Weapon);
            AddIfMissing("Baseball Bat", SupplyGroup.Weapon);
            AddIfMissing("Pepper Spray", SupplyGroup.Weapon);
            AddIfMissing("Taser", SupplyGroup.Weapon);
            AddIfMissing("Hunting Bow", SupplyGroup.Weapon);
            AddIfMissing("Handgun", SupplyGroup.Weapon);
            AddIfMissing("Ammo Box", SupplyGroup.Weapon);

            AddIfMissing("Hammer", SupplyGroup.Tool);
            AddIfMissing("Screwdriver Set", SupplyGroup.Tool);
            AddIfMissing("Wrench", SupplyGroup.Tool);
            AddIfMissing("Drill", SupplyGroup.Tool);
            AddIfMissing("Tape Measure", SupplyGroup.Tool);
            AddIfMissing("Ladder", SupplyGroup.Tool);

            AddIfMissing("Toothbrush", SupplyGroup.Hygiene);
            AddIfMissing("Toothpaste", SupplyGroup.Hygiene);
            AddIfMissing("Shampoo", SupplyGroup.Hygiene);
            AddIfMissing("Conditioner", SupplyGroup.Hygiene);
            AddIfMissing("Body Wash", SupplyGroup.Hygiene);
            AddIfMissing("Deodorant", SupplyGroup.Hygiene);
            AddIfMissing("Lotion", SupplyGroup.Hygiene);
            AddIfMissing("Razors", SupplyGroup.Hygiene);
            AddIfMissing("Hand Soap", SupplyGroup.Hygiene);
            AddIfMissing("First Aid Kit", SupplyGroup.Hygiene);

            AddIfMissing("Electronics Store", SupplyGroup.Store);
            AddIfMissing("Toy Store", SupplyGroup.Store);
            AddIfMissing("Sporting Goods Store", SupplyGroup.Store);
            AddIfMissing("Pharmacy", SupplyGroup.Store);
            AddIfMissing("Big Box Retailer", SupplyGroup.Store);
        }

        private void AddIfMissing(string name, SupplyGroup group)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (HasSupply(name, group))
            {
                return;
            }

            supplies.Add(new SupplyItem { Name = name, Group = group });
        }

        private static SupplyItem CreateAnimal(string species)
        {
            return new SupplyItem
            {
                Name = species,
                Group = SupplyGroup.Animal,
                Species = species
            };
        }

        private static SupplyItem CreateAnimalBreed(string species, string breed)
        {
            return new SupplyItem
            {
                Name = $"{breed} {species}",
                Group = SupplyGroup.Animal,
                Species = species,
                Breed = breed
            };
        }

        public IReadOnlyList<SupplyItem> Supplies => supplies;

        public List<SupplyItem> GetByGroup(SupplyGroup group)
        {
            return supplies.FindAll(s => s.Group == group);
        }

        public List<SupplyItem> GetAnimalsBySpecies(string species)
        {
            if (string.IsNullOrWhiteSpace(species))
            {
                return new List<SupplyItem>();
            }

            return supplies.FindAll(s => s != null && s.Group == SupplyGroup.Animal &&
                string.Equals(s.SpeciesOrName, species, StringComparison.OrdinalIgnoreCase));
        }

        public SupplyItem GetAnimalBreed(string species, string breed)
        {
            if (string.IsNullOrWhiteSpace(species) || string.IsNullOrWhiteSpace(breed))
            {
                return null;
            }

            return supplies.Find(s => s != null && s.Group == SupplyGroup.Animal &&
                string.Equals(s.SpeciesOrName, species, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(s.Breed, breed, StringComparison.OrdinalIgnoreCase));
        }

        public SupplyItem GetRandomByGroup(SupplyGroup group)
        {
            List<SupplyItem> matches = GetByGroup(group);
            if (matches == null || matches.Count == 0)
            {
                return null;
            }

            return matches[UnityEngine.Random.Range(0, matches.Count)];
        }

        public List<SupplyGroup> GetPriorityGroupsForCharacter(CharacterCore character)
        {
            List<SupplyGroup> groups = new();
            if (character == null)
            {
                return groups;
            }

            AddGroupIfMissing(groups, SupplyGroup.Hygiene);
            AddGroupIfMissing(groups, SupplyGroup.Consumable);
            AddGroupIfMissing(groups, character.CurrentLifeStage is LifeStage.Baby or LifeStage.Infant or LifeStage.Toddler or LifeStage.Child ? SupplyGroup.Toy : SupplyGroup.Accessory);
            AddGroupIfMissing(groups, character.CurrentLifeStage is LifeStage.Teen or LifeStage.YoungAdult or LifeStage.Adult ? SupplyGroup.Electronics : SupplyGroup.Household);

            switch (character.ClothingStyle)
            {
                case ClothingStyleType.Work:
                case ClothingStyleType.Formal:
                case ClothingStyleType.Medical:
                    AddGroupIfMissing(groups, SupplyGroup.Clothing);
                    AddGroupIfMissing(groups, SupplyGroup.Tool);
                    break;
                case ClothingStyleType.Streetwear:
                case ClothingStyleType.Festival:
                    AddGroupIfMissing(groups, SupplyGroup.Accessory);
                    AddGroupIfMissing(groups, SupplyGroup.Trinket);
                    break;
                case ClothingStyleType.Utility:
                case ClothingStyleType.Outdoor:
                    AddGroupIfMissing(groups, SupplyGroup.Tool);
                    AddGroupIfMissing(groups, SupplyGroup.Household);
                    break;
            }

            if (character.Talents != null)
            {
                if (character.Talents.Contains(CharacterTalent.Technical))
                {
                    AddGroupIfMissing(groups, SupplyGroup.Electronics);
                }

                if (character.Talents.Contains(CharacterTalent.Caregiving))
                {
                    AddGroupIfMissing(groups, SupplyGroup.Medicine);
                    AddGroupIfMissing(groups, SupplyGroup.Hygiene);
                }

                if (character.Talents.Contains(CharacterTalent.Athletic))
                {
                    AddGroupIfMissing(groups, SupplyGroup.Toy);
                }
            }

            return groups;
        }

        public List<SupplyItem> GetSuggestedSuppliesForCharacter(CharacterCore character, int maxItems = 6)
        {
            List<SupplyItem> suggestions = new();
            List<SupplyGroup> groups = GetPriorityGroupsForCharacter(character);
            for (int i = 0; i < groups.Count && suggestions.Count < maxItems; i++)
            {
                List<SupplyItem> candidates = GetByGroup(groups[i]);
                if (candidates == null || candidates.Count == 0)
                {
                    continue;
                }

                int index = Mathf.Abs((character.CharacterId?.GetHashCode() ?? 0) + (i * 17)) % candidates.Count;
                suggestions.Add(candidates[index]);
            }

            return suggestions;
        }

        private static void AddGroupIfMissing(List<SupplyGroup> groups, SupplyGroup group)
        {
            if (!groups.Contains(group))
            {
                groups.Add(group);
            }
        }
    }
}
