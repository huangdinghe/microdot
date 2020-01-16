﻿#region Copyright 
// Copyright 2017 Gigya Inc.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License.  
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Gigya.Common.Contracts.Exceptions;
using Gigya.Microdot.Interfaces.Configuration;
using Gigya.Microdot.Interfaces.SystemWrappers;
using Gigya.Microdot.LanguageExtensions;
using Gigya.Microdot.SharedLogic.Utils;

namespace Gigya.Microdot.SharedLogic.SystemWrappers
{
    [ConfigurationRoot("dataCenters", RootStrategy.ReplaceClassNameWithPath)]
    public class DataCentersConfig : IConfigObject
    {
        public string Current { get; set; }
    }

    public class HostConfiguration : IEnvironment
    {
        private readonly string _region;

        private const string GIGYA_CONFIG_ROOT_DEFAULT = "config";
        private const string LOADPATHS_JSON = "loadPaths.json";

        private const string GIGYA_CONFIG_ROOT_KEY = "GIGYA_CONFIG_ROOT";
        private const string GIGYA_CONFIG_PATHS_FILE_KEY = "GIGYA_CONFIG_PATHS_FILE";

        private Func<DataCentersConfig> GetDataCentersConfig { get; }


        public HostConfiguration(params IHostConfigurationSource[] sources)
            : this(sources as IEnumerable<IHostConfigurationSource>) { }
        
        public HostConfiguration(IEnumerable<IHostConfigurationSource> sources)
        {
            ConfigRoot = GetDefaultConfigRoot();
            LoadPathsFile = GetDefaultPathsFile();
            
            foreach (var s in sources)
            {
                Zone                  = s.Zone                  ?? Zone;
                Region                = s.Region                ?? Region;
                DeploymentEnvironment = s.DeploymentEnvironment ?? DeploymentEnvironment;
                ConsulAddress         = s.ConsulAddress         ?? ConsulAddress;
                ApplicationInfo       = s.ApplicationInfo       ?? ApplicationInfo;
                ConfigRoot            = s.ConfigRoot            ?? ConfigRoot;
                LoadPathsFile         = s.LoadPathsFile         ?? LoadPathsFile;
            }

            if (Zone                  == null) throw new ArgumentNullException($"{ nameof(Zone)                  } wasn't supplied.");
            if (Region                == null) throw new ArgumentNullException($"{ nameof(Region)                } wasn't supplied.");
            if (DeploymentEnvironment == null) throw new ArgumentNullException($"{ nameof(DeploymentEnvironment) } wasn't supplied.");
            if (ConsulAddress         == null) throw new ArgumentNullException($"{ nameof(ConsulAddress)         } wasn't supplied.");
            if (ApplicationInfo       == null) throw new ArgumentNullException($"{ nameof(ApplicationInfo)       } wasn't supplied.");

            if (ConfigRoot.Exists == false)
            {
                throw new EnvironmentException(
                    $"ConfigRoot path doesn't exist '{ ConfigRoot.FullName }'. " +
                    $"Use '{GIGYA_CONFIG_ROOT_KEY}' environment variable to override default path.");
            }

            if (LoadPathsFile.Exists == false)
            {
                throw new EnvironmentException(
                    $"LoadPaths file isn't found at '{ LoadPathsFile.FullName }'. " +
                    $"Use '{GIGYA_CONFIG_PATHS_FILE_KEY}' environment variable to define absolute path" +
                    $"to the file or place a 'loadPaths.json' at your config root.");
            }
        }

        private DirectoryInfo GetDefaultConfigRoot() =>
            Path.Combine(Environment.CurrentDirectory, GIGYA_CONFIG_ROOT_DEFAULT)
                .To(x => new DirectoryInfo(x));

        private FileInfo GetDefaultPathsFile() =>
            Path.Combine(ConfigRoot.FullName, LOADPATHS_JSON)
                .To(x => new FileInfo(x));


        [Obsolete("Use the ApplicationInfo property instead. Will be removed in 3.0.")]
        public string InstanceName => ApplicationInfo.Name;

        public string Zone { get; }
        public string Region { get; } //_region ?? GetDataCentersConfig().Current; // if environmentVariable %REGION% does not exist, take the region from DataCenters configuration (the region was previously called "DataCenter")
        public string DeploymentEnvironment { get; }
        public string ConsulAddress { get; }
        public DirectoryInfo ConfigRoot { get; }
        public FileInfo LoadPathsFile { get; }
        public CurrentApplicationInfo ApplicationInfo { get; }
    }
}