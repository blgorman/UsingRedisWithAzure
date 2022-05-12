# DN6 Simple Web with Auth and Insights

This is a simple template with Auth and Insights for DotNet 6 MVC.  I've also added app configuration and code to integrate Azure App Config and Azure Key Vault.  If you try to use those, make sure to read all the notes in the Program.cs file.  It would also help if you've seen the video from the Opsgility Azure A-Z conference.

## Use at your own risk

We are not responsible for anything you do with this code.

## Set Up the solution

You will need to add your Application insights at Azure and you will need to wire up the Instrumentation connection string.

Additionally, you will need SQL Developer locally, then you will need to do some configuration and create and wire up the SQLAzure at Azure.

## Notes

Additional Thoughts

### YAML

Use the sample YAML at your discretion.

You will want to update your user secret to map correctly for your publish profile and you will want to update the name of your azure web app

### Developer Secrets

As you develop the solution, do not store your connection strings in the appsettings.json file.

Instead, put them in the user secrets.

You will likely want a separate instance of application insights for your developer from your web/slot in production in the real world.  You could use the same one for demo/learning purposes.
