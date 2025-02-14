import React from 'react';
import { Route, Routes } from 'react-router-dom';
import EmployeeList from './Employee/EmployeeList';
import EmployeeForm from './Employee/EmployeeForm';
import TimesheetList from './TimeSheets/TimeSheetList';
import TimesheetForm from './TimeSheets/TimeSheetForm';

function App() {
  return (
    <div className="App">
      <Routes>
        <Route path="/Employees" element={<EmployeeList />} />
        <Route path="/Employees/new" element={<EmployeeForm />} />
        <Route path="/Employees/:id" element={<EmployeeForm />} />
        <Route path="/Timesheets" element={<TimesheetList />} />
        <Route path="/Timesheets/new" element={<TimesheetForm />} />
        <Route path="/Timesheets/:id" element={<TimesheetForm />} />
      </Routes>
    </div>
  );
}

export default App;
