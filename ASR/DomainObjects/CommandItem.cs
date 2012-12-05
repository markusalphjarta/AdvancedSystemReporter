using System.Collections.Generic;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Data.Items;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Reflection;

namespace ASR.DomainObjects
{
    public class CommandItem : BaseItem
    {
        public CommandItem(Item innerItem)
            : base(innerItem)
        {
        }

        public string Title
        {
            get { return InnerItem["Title"]; }
        }

        public new string Icon
        {
            get { return InnerItem["Icon"]; }
        }


        public string Command
        { get { return InnerItem["Command"]; } }


        public bool SingleItemContext
        {
            get { return InnerItem["Command"] == "1"; }
        }

        internal void Run(StringList values)
        {
            if (string.IsNullOrEmpty(Command))
            {
                return;
            }

            var items = new List<Item>();
            var othervalues = new StringList();

            foreach (var val in values)
            {
                var uri = ItemUri.Parse(val);
                if (uri != null)
                {
                    items.Add(Database.GetItem(uri));
                }
                else
                {
                    othervalues.Add(val);
                }
            }

            Command command = CommandManager.GetCommand(Command)
                ?? (Command)ReflectionUtil.CreateObject(Command);

            Assert.IsNotNull(command, Command + " not found.");

            //pass parameters
            var indexSt = Command.IndexOf('(') + 1;
            if (indexSt > 0)
            {
                var length = Command.IndexOf(')') - indexSt;
                if (length > 0)
                {
                    ReflectionUtil.SetProperties(command, Command.Substring(indexSt, length));
                }
            }

            // If our command can hanlde more than one item in the context we run it once
            if (!SingleItemContext)
            {
                var cc = new CommandContext(items.ToArray()) { CustomData = othervalues };
                command.Execute(cc);
            }
            //otherwise we have to generate as many commands as items 
            else
            {
                if (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        var cc = new CommandContext(item);
                        command.Execute(cc);
                    }
                }
                if (othervalues.Count > 0)
                {
                    foreach (var othervalue in othervalues)
                    {
                        var cc = new CommandContext { CustomData = othervalue };
                        command.Execute(cc);
                    }
                }
            }
        }
    }
}

