using NUnit.Framework;
using UnityEngine;
using Survivebest.Core;
using System.Reflection;

namespace Survivebest.Tests.EditMode
{
    public class SaveSchemaMigrationTests
    {
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
