var sep = require('path').sep;

module.exports = function (grunt) {
    grunt.initConfig({
        pkg: grunt.file.readJSON("package.json"),
        bump: {
            options: {
                files: ['package.json'],
                updateConfigs: ['pkg'],
                commit: false,
                createTag: false,
                push: false
            }
        },
        msbuild: {
            release: {
                src: ['src/NCDO/NCDO.csproj'],
                options: {
                    projectConfiguration: 'Release',
                    targets: ['Clean', 'Rebuild'],
                    buildParameters: {
                        OutputPath: process.cwd() + sep + "build",
                        WarningLevel: 2,
                        FileVersion: '<%= pkg.version %>',
                        Version: '<%= pkg.version %>',
                        AssemblyVersion: '<%= pkg.version %>'
                    },
                }
            }
        },
        nugetpush: {
            dist: {
                src: 'build/NCDO.<%= pkg.version %>.nupkg',
                options: {
                    source: "https://www.nuget.org"
                }
            },
        },
        availabletasks: {
            tasks: {
                options: {
                    filter: 'exclude',
                    tasks: ['availabletasks', 'default'],
                    showTasks: ['user']
                }
            }
        }
    });

// Load the plugin that provides the tasks.
    require('load-grunt-tasks')(grunt);
    grunt.registerTask('Bump', ["bump"]);
    grunt.registerTask('Publish', ["tag", "msbuild", "nugetpush"]);
    grunt.registerTask('default', ["availabletasks"]);
};