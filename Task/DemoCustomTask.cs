using Microsoft.Build.Utilities;

using System;

namespace DemoCustomTask
{
    public class DemoCustomTask : Task
    {
        public override bool Execute()
        {
            Log.LogMessage(Microsoft.Build.Framework.MessageImportance.High, "Hello from a custom task!");

            return !Log.HasLoggedErrors;
        }
    }
}
