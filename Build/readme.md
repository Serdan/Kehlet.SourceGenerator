Reasonable defaults for C# Source Generator projects. Includes a AvoidCycleErrorOnSelfReference property as a work-around for cyclic dependencies.

props:
```
<Project>
    <PropertyGroup>
        <IsPackable>true</IsPackable>
        <Nullable>enable</Nullable>
        <IncludeBuildOutput>false</IncludeBuildOutput>
        <ImplicitUsings>true</ImplicitUsings>
        <NoWarn>$(NoWarn);NU5128</NoWarn>
        <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
        <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
        <IsRoslynComponent>true</IsRoslynComponent>
        <DevelopmentDependency>true</DevelopmentDependency>
    </PropertyGroup>

    <ItemGroup>
        <None Include="$(OutputPath)\$(AssemblyName).dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
    </ItemGroup>
</Project>
```

targets:
```
<Project>
    <Choose>
        <When Condition="$(AvoidCycleErrorOnSelfReference) == 'true'">
            <PropertyGroup>
                <PackageId Condition="'$(PackageId)' == ''">$(MSBuildProjectName)</PackageId>
                <PackageIdTemp>$(PackageId)</PackageIdTemp>
                <PackageId>$(PackageId)_temp</PackageId>
            </PropertyGroup>
        </When>
    </Choose>

    <Target Name="_UpdatePackageId" BeforeTargets="$(PackDependsOn)" Condition="$(AvoidCycleErrorOnSelfReference) == 'true'">
        <PropertyGroup>
            <PackageId>$(PackageIdTemp)</PackageId>
        </PropertyGroup>
    </Target>
</Project>
```