using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using System.Reflection;

namespace Survivebest.Tests.EditMode
{
    public class SaveSchemaMigrationTests
    {

        [Test]
        public void Version2Payload_IsMigratedToVersion3()
        {
            GameObject go = new GameObject("SaveGameManagerTestV2");
            SaveGameManager manager = go.AddComponent<SaveGameManager>();

            SaveSlotPayload payload = new SaveSlotPayload
            {
                SchemaVersion = 2,
                WorldName = "V2World"
            };

            MethodInfo migrate = typeof(SaveGameManager).GetMethod("MigratePayloadIfNeeded", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(migrate);

            SaveSlotPayload migrated = (SaveSlotPayload)migrate.Invoke(manager, new object[] { payload });
            Assert.IsNotNull(migrated);
            Assert.AreEqual(3, migrated.SchemaVersion);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void LegacyPayload_IsMigratedToCurrentVersion()
        {
            GameObject go = new GameObject("SaveGameManagerTest");
            SaveGameManager manager = go.AddComponent<SaveGameManager>();

            SaveSlotPayload payload = new SaveSlotPayload
            {
                SchemaVersion = 1,
                WorldName = "LegacyWorld"
            };

            MethodInfo migrate = typeof(SaveGameManager).GetMethod("MigratePayloadIfNeeded", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(migrate);

            SaveSlotPayload migrated = (SaveSlotPayload)migrate.Invoke(manager, new object[] { payload });
            Assert.IsNotNull(migrated);
            Assert.AreEqual(3, migrated.SchemaVersion);

            Object.DestroyImmediate(go);
        }
    }
}
