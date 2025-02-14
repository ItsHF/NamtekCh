import React, { useState } from 'react';
import axios from 'axios';
import styles from './EmployeeForm.css';

const EmployeeForm = () => {
  const [employee, setEmployee] = useState({
    name: '',
    email: '',
    phoneNumber: '',
    dateOfBirth: '',
    jobTitle: '',
    department: '',
    salary: 1500, // Set minimum default salary
    startDate: new Date().toISOString().split('T')[0], // Current date
    endDate: '', // Can be left empty
  });

  const [files, setFiles] = useState({
    image: null,
    cv: null,
    idCard: null,
  });

  const [preview, setPreview] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState(null);
  const [error, setError] = useState(null);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setEmployee((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleFileChange = (e) => {
    const { name, files } = e.target;
    if (files.length > 0) {
      setFiles((prev) => ({
        ...prev,
        [name]: files[0],
      }));

      if (name === 'image') {
        const fileReader = new FileReader();
        fileReader.onload = () => {
          setPreview(fileReader.result);
        };
        fileReader.readAsDataURL(files[0]);
      }
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setMessage(null);
    setIsSubmitting(true);
  
    try {
      // Convert files to base64
      const photoBase64 = files.image ? await convertToBase64(files.image) : null;
      const cvBase64 = files.cv ? await convertToBase64(files.cv) : null;
      const idCardBase64 = files.idCard ? await convertToBase64(files.idCard) : null;
  
      const payload = {
        employee: {
          name: employee.name,
          email: employee.email,
          phoneNumber: employee.phoneNumber,
          dateOfBirth: employee.dateOfBirth,
          jobTitle: employee.jobTitle,
          department: employee.department,
          salary: employee.salary,
          startDate: employee.startDate,
          endDate: employee.endDate.trim() === "" ? null : employee.endDate,
        },
        photo: photoBase64,
        cv: cvBase64,
        idCard: idCardBase64,
      };
  
      const response = await axios.post('https://localhost:7035/api/Employee', payload, {
        headers: { 'Content-Type': 'application/json' }
      });
  
      if (response.status === 201) {
        setMessage('Employee created successfully!');
        // Reset form
        setEmployee({
          name: '', email: '', phoneNumber: '', dateOfBirth: '',
          jobTitle: '', department: '', salary: 1500, startDate: new Date().toISOString().split('T')[0], endDate: null
        });
        setFiles({ image: null, cv: null, idCard: null });
        setPreview(null);
      } else {
        setError('Failed to create employee');
      }
    } catch (err) {
      if (err.response && err.response.data) {
        // Extract validation messages from API response
        const apiErrors = err.response.data;
        if (typeof apiErrors === 'string') {
          setError(apiErrors); // If error is a plain string message
        } else if (Array.isArray(apiErrors)) {
          setError(apiErrors.join(', ')); // If error is an array of messages
        } else if (typeof apiErrors === 'object') {
          const messages = Object.values(apiErrors).flat().join(', ');
          setError(messages); // If errors are in a dictionary format
        } else {
          setError('An unexpected error occurred.');
        }
      } else {
        setError('Error submitting the form. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };
  
  
  // Helper function to convert file to base64
  const convertToBase64 = (file) => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => resolve(reader.result);
      reader.onerror = (error) => reject(error);
    });
  };

  return (
    <div className="container">
      <h2>Create Employee</h2>

      {error && <div className="error-message">{error}</div>}
  {message && <div className="success-message">{message}</div>}

      <form onSubmit={handleSubmit} className="form">
        <div className="grid">
          <label>Name</label>
          <input type="text" name="name" value={employee.name} onChange={handleChange} required />

          <label>Email</label>
          <input type="email" name="email" value={employee.email} onChange={handleChange} required />

          <label>Phone Number</label>
          <input type="tel" name="phoneNumber" value={employee.phoneNumber} onChange={handleChange} required />

          <label>Date of Birth</label>
          <input type="date" name="dateOfBirth" value={employee.dateOfBirth} onChange={handleChange} required />

          <label>Job Title</label>
          <input type="text" name="jobTitle" value={employee.jobTitle} onChange={handleChange} required />

          <label>Department</label>
          <input type="text" name="department" value={employee.department} onChange={handleChange} required />
          <label>Salary</label>
        <input 
          type="number" 
          name="salary" 
          value={employee.salary} 
          onChange={handleChange} 
          min="1500" 
          required 
        />

        <label>Start Date</label>
        <input 
          type="date" 
          name="startDate" 
          value={employee.startDate} 
          onChange={handleChange} 
          required 
        />

        <label>End Date (Optional)</label>
        <input 
          type="date" 
          name="endDate" 
          value={employee.endDate} 
          onChange={handleChange} 
        />
        </div>

        <div className="file-section">
          <label>Upload Image</label>
          <input type="file" name="image" onChange={handleFileChange} accept=".jpg,.jpeg,.png" />
          {preview && <img src={preview} alt="Preview" className="preview" />}

          <label>Upload CV</label>
          <input type="file" name="cv" onChange={handleFileChange} accept=".pdf,.doc,.docx" />

          <label>Upload ID Card</label>
          <input type="file" name="idCard" onChange={handleFileChange} accept="image/*,.pdf" />
        </div>

        <button type="submit" disabled={isSubmitting} className="submit-btn">
          {isSubmitting ? 'Creating...' : 'Create Employee'}
        </button>
      </form>
    </div>
  );
};

export default EmployeeForm;
