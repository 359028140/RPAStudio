﻿using System;
using System.Activities;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace RPAStudio.Librarys
{
    public class DynamicActivityGenerator
    {
        private const string _templateTypePrefix = "";
        private AssemblyBuilder _ab;
        private ModuleBuilder _mb;
        private readonly string _fileName;
        private readonly string _moduleName;
        private Dictionary<string, Type> _types;
        public DynamicActivityGenerator(string name)
        {
            _types = new Dictionary<string, Type>();
            _fileName = String.Format("{0}.dll", name);
            _moduleName = name;

            _ab = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName(_moduleName), AssemblyBuilderAccess.RunAndSave, System.IO.Path.GetTempPath());
            _mb = _ab.DefineDynamicModule(_moduleName, _fileName);
        }
        private string GetTypeName(string prefix, string workflowName)
        {
            var baseName = String.Format("{0}{1}", prefix, workflowName);
            var name = baseName;
            var index = 1;

            while (_types.ContainsKey(name))
            {
                name = String.Format("{0}{1}", baseName, index);
                index++;
            }

            return name;
        }
        public void Save()
        {
            _ab.Save(_fileName);
        }
        public Type AppendSubWorkflowTemplateFromFile(string workflowName, string path)
        {
            return AppendSubWorkflowTemplate(workflowName, LoadXamlFromFile(path));
        }
        public Type AppendSubWorkflowTemplate(string workflowName, Activity activity)
        {
            return AppendSubWorkflowTemplate(workflowName, System.Xaml.XamlServices.Save(activity));
        }
        public Type AppendSubWorkflowTemplate(string workflowName, string xaml)
        {
            var name = GetTypeName(_templateTypePrefix, workflowName);
            var tb = _mb.DefineType(name, TypeAttributes.Public | TypeAttributes.Class, typeof(DynamicActivityTemplateFactory));
            var methodb = tb.DefineMethod("GetActivity", MethodAttributes.Public | MethodAttributes.Virtual, typeof(string), null);
            var msil = methodb.GetILGenerator();
            msil.Emit(OpCodes.Ldstr, xaml);
            msil.Emit(OpCodes.Ret);
            var t = tb.CreateType();
            _types.Add(name, t);
            return t;
        }
        public void Load()
        {
            _ab.Save(_fileName);
            Assembly.Load(new AssemblyName(_moduleName));
        }
        public DynamicActivityDescriptor[] Activities
        {
            get
            {
                return _types.Select(descriptor => new DynamicActivityDescriptor(descriptor.Key, descriptor.Value)).ToArray();
            }
        }
        private string LoadXamlFromFile(string path)
        {
            var xml = new System.Xml.XmlDocument();
            xml.Load(path);

            if (xml.DocumentElement == null)
                throw new InvalidOperationException("Unable to load Component. Component has invalid content.");

            return xml.DocumentElement.InnerXml;
        }
    } // class DynamicActivityGenerator}
    public class DynamicActivityDescriptor
    {
        public DynamicActivityDescriptor(string name, Type type)
        {
            Name = name;
            Type = type;
        }
        public string Name { get; set; }
        public Type Type { get; set; }
    } // class DynamicActivityDescriptor
    //[System.Drawing.ToolboxBitmap(typeof(ResFinder), "Resources.toolbox.workflow.png")]
    public abstract class DynamicActivityTemplateFactory : IActivityTemplateFactory
    {
        public virtual string GetActivity()
        {
            return null;
        }
        public Activity Create(System.Windows.DependencyObject target)
        {
            try
            {
                DynamicActivity activity = null;
                var a = GetActivity();
                var s = new System.IO.StringReader(a);
                // activity = System.Xaml.XamlServices.Load(s) as DynamicActivity;
                activity = (DynamicActivity)System.Activities.XamlIntegration.ActivityXamlServices.Load(s);
                return activity == null || activity.Implementation == null ? null : activity.Implementation();
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString());
                throw;
            }
        }
        
    }

}
