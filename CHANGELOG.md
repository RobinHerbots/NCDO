# Change Log
All notable changes to this project will be documented in this file.

## [0.0.22 - 2017-12-04]
### Added
- add rowstate to cdo_record

## [0.0.21 - 2017-11-23]
### fixed
- autoapplychanges fix

## [0.0.20 - 2017-11-23]
### added
- hookup autoapplychanges

## [0.0.19 - 2017-11-21]
### added
- full CRUD support in CDO
- track newly created records

### update
- Make CDO_Record & CDO_Table track full changes

### fixed
- Add support for capabilities detection #3

## [0.0.18 - 2017-11-14]
### fixed
- filterexpression allow null for ablfilter
- Fix in determinemaintable

## [0.0.17 - 2017-11-07]
### update
- add changetracking in cdo_record

### fixed
- stackoverflow bug

## [0.0.16 - 2017-11-07]
### added
- support for datadefinitions schema
### update
- extend record with INotifyPropertyChanged & table with INotifyCollectionChanged

## [0.0.15 - 2017-11-06]
### added
- version prop in cdosession
### updated
- allow joining of records in memory

## [0.0.14 - 2017-11-02]
### updated
- improve inmemory data

## [1.0.13 - 2017-10-31]
### updated
- add missing Comparison operators to ablfilter

## [1.0.12 - 2017-10-31]
### updated
- extend find & findbyid with autoFetch parameter
- extend CDO with  Fill & Read with ExpressionFilter;

## [1.0.11 - 2017-10-27]
### updated
- find & findbyid => typed search

## [1.0.10 - 2017-10-26]
### added
- find & findbyid
- allow string as filter parameter for fill & read

## [1.0.9 - 2017-10-24]
### added
- reuse of httpclient between session and cdo's
