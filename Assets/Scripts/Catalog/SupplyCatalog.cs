using System;
using System.Collections.Generic;
using UnityEngine;

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
        Consumable
    }

    [Serializable]
    public class SupplyItem
    {
        public string Name;
        public SupplyGroup Group;
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

            // Animals (35)
            new SupplyItem { Name = "Dog", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Cat", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Horse", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Cow", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Pig", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Sheep", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Goat", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Rabbit", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Rat", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Mouse", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Squirrel", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Deer", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Bear", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Wolf", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Fox", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Lion", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Tiger", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Elephant", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Giraffe", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Zebra", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Kangaroo", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Koala", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Panda", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Monkey", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Gorilla", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Chimpanzee", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Dolphin", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Whale", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Shark", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Eagle", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Owl", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Hawk", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Falcon", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Penguin", Group = SupplyGroup.Animal },
            new SupplyItem { Name = "Flamingo", Group = SupplyGroup.Animal },

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

        public IReadOnlyList<SupplyItem> Supplies => supplies;

        public List<SupplyItem> GetByGroup(SupplyGroup group)
        {
            return supplies.FindAll(s => s.Group == group);
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
    }
}
