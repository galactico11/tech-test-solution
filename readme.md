This solution was developed with Visual Studio Code and has not been tested in Visual Studio

Given more time I would:

- Improve handling data away from the "happy path" - for example, if no headers are supplied in CSV, and render error in UI
- Improve the UX to highlight to the user which rows are incorrect
- Add some UI for browing the database data
- Add further unit tests to cover individual erroneous data cases
- Add a real database and connection, which would just require swapping out the `AccountRepository` implementation
