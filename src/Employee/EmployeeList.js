import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

const EmployeeList = () => {
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Fetch the list of employeess
  useEffect(() => {
    fetch('https://localhost:7035/api/Employee')
      .then(response => response.json())
      .then(data => {
        setEmployees(data);
        setLoading(false); // Stop loading after data is fetched
      })
      .catch(err => {
        setError('Failed to load employees');
        setLoading(false); // Stop loading even on error
        console.error('Error fetching employees:', err);
      });
  }, []);

  if (loading) {
    return <div>Loading...</div>; // Show loading message or spinner
  }

  return (
    <div>
      <h1>Employee List</h1>
      {error && <div style={{ color: 'red' }}>{error}</div>} {/* Show error message if any */}
      
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Phone Number</th>
            <th>Job Title</th>
            <th>Salary</th>
            <th>Start Date</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {employees.map(employee => (
            <tr key={employee.id}>
              <td>{employee.name}</td>
              <td>{employee.email}</td>
              <td>{employee.phoneNumber}</td>
              <td>{employee.jobTitle}</td>
              <td>{employee.salary}</td>
              <td>{new Date(employee.startDate).toLocaleDateString()}</td>
              <td>
                <Link to={`/Employees/${employee.id}`}>View</Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      
      <Link to="/Employees/new">Add New Employee</Link>
    </div>
  );
};

export default EmployeeList;
